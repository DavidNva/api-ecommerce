# Repositorio — Consultas: Get / Search / Exists

## La pregunta define lo que devuelve el método

El tipo de retorno depende de **qué tan único es el criterio** por el que buscas, no de que el método se llame "Product".

| Criterio | ¿Único? | Devuelve | Por qué |
|---|---|---|---|
| `ProductId` | Sí (PK) | `Product?` | Máximo 1 fila |
| `SKU` (con índice único) | Sí | `Product?` | Igual que el id |
| `Name` | **No** | `IReadOnlyList<Product>` | "Playera negra" puede existir en varias tallas o categorías |
| `CategoryId` | No | `IReadOnlyList<Product>` | Una categoría tiene muchos productos |

**Analogía:** pedir "el cliente con RFC X" te da una persona. Pedir "los clientes que se llaman Juan" te da una lista, aunque hoy solo exista un Juan, porque mañana puede haber dos.

El tipo de retorno es un **contrato**. Si buscando por nombre devolvieras un solo `Product?`, tendrías que elegir cuál con `FirstOrDefault` y perderías el resto. Con `SingleOrDefault` truena en cuanto haya dos.

### Nombres

| Nombre | Comentario |
|---|---|
| `GetProduct(id)` | Singular, devuelve uno ✅ |
| `GetProducts()` | Plural, devuelve varios ✅ |
| `SearchProduct(name)` | ❌ Singular pero devuelve lista. Mejor `SearchProducts` |

Si necesitas "un producto por criterio de negocio", no cambies `Search`: agrega un método con criterio único, como `GetProductBySku(string sku)` que devuelva `Product?`.

---

## `Search` vs `Exists`

No compiten, responden preguntas distintas.

| | `SearchProducts` | `ProductExists` |
|---|---|---|
| Pregunta | "Dame los que coincidan" | "¿Hay al menos uno?" |
| Devuelve | Lista de entidades completas | `bool` |
| SQL | `SELECT` de todas las columnas con `ORDER BY` | `SELECT CASE WHEN EXISTS(...)`, se detiene en el primer match |
| Datos que viajan | Todas las filas y columnas | Un bit |
| Uso típico | Buscador de la tienda | Validar antes de crear |

**Analogía:** `Exists` es preguntarle al cajero "¿tienen playeras negras?" y recibir un sí o un no. `Search` es pedirle que saque todas las playeras negras de la bodega y las ponga en el mostrador.

Para saber si algo existe, usa `Any()`. No uses `Search(...).Any()` ni `.Count() > 0`: traen datos que no necesitas.

---

## Problemas de `SearchProduct` (versión actual)

```csharp
public ICollection<Product> SearchProduct(string name)
{
    IQueryable<Product> query = _db.Products;
    if (!string.IsNullOrWhiteSpace(name))
    {
        query = query.Where(p => p.Name.ToLower().Trim() == name.ToLower().Trim());
    }
    return query.OrderBy(p => p.Name).ToList();
}
```

| Prioridad | Problema | Por qué |
|---|---|---|
| 🔴 | Con `name` vacío devuelve **todo el catálogo** | Sin paginación puede tumbar la API. Además es inconsistente con `ProductExists`, que devuelve `false`. |
| 🔴 | Se llama "search" pero hace igualdad exacta (`==`) | Buscar "playera" no encuentra "Playera negra". Un buscador usa `Contains`. |
| 🟠 | `ToLower().Trim()` dentro del `Where` | Se traduce a `LOWER(LTRIM(RTRIM(Name)))` por fila. No aprovecha índices y con collation CI no hace falta. |
| 🟠 | Sin `AsNoTracking()` | Es solo lectura. El tracking es costo sin beneficio. |
| 🟡 | Síncrono y devuelve `ICollection` | Mejor `Task<IReadOnlyList<Product>>` con `ToListAsync()`. |

**Lo que sí está bien:** armar el `IQueryable`, agregar el filtro opcional con `if` y materializar al final con `ToList()`.

### Versión recomendada (ejemplo)

```csharp
public async Task<IReadOnlyList<Product>> SearchProductsAsync(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return Array.Empty<Product>();   // o paginar el catálogo, es decisión de diseño

    return await _db.Products
        .AsNoTracking()
        .Where(p => p.Name.Contains(name.Trim()))
        .OrderBy(p => p.Name)
        .ToListAsync();
}
```

**Decisión de diseño:** con texto vacío, ¿lista vacía o catálogo paginado (`Skip/Take`)? Para un buscador de tienda se suele paginar. Para una validación interna, lista vacía.

---

## `==` vs `Contains` vs `StartsWith` y los índices

| Operador | SQL | ¿Usa índice? |
|---|---|---|
| `p.Name == x` | `Name = @x` | ✅ Sí |
| `p.Name.StartsWith(x)` | `LIKE 'x%'` | ✅ Sí |
| `p.Name.Contains(x)` | `LIKE '%x%'` | ❌ No (el comodín va al inicio) |
| `p.Name.ToLower() == x` | `LOWER(Name) = @x` | ❌ No (función sobre la columna) |

Para catálogos grandes con búsqueda por texto libre, lo serio es Full-Text Search de SQL Server. Para el curso, `Contains` es suficiente.

### `ToLower().Trim()` dentro de la consulta

SQL Server compara sin distinguir mayúsculas por defecto (collation CI), así que `p.Name == name.Trim()` basta. El `Trim()` se aplica al **parámetro** (en C#), no a la columna.

---

## `GetProduct` y `GetProductsForCategory`

Estos filtran por **llave**, y eso es mucho mejor que filtrar por texto.

| | `GetProduct(id)` | `GetProductsForCategory(id)` | `SearchProducts(name)` |
|---|---|---|---|
| Filtra por | PK | FK | Texto |
| Índice | Sí (clustered) | Sí (EF lo crea para FKs por convención) | No lo aprovecha |
| Resultado | 0 o 1 | N filas | N filas |

| Método | Observación |
|---|---|
| `GetProduct` | `FirstOrDefault(p => p.ProductId == id)` funciona. Como filtras por PK, `Find` / `FindAsync(id)` es más natural: revisa primero el change tracker y evita ir a la BD si ya está cargado. |
| `GetProductsForCategory` | Bien: filtra y ordena en la consulta antes del `ToList`. Le faltan `AsNoTracking`, async y paginación. |

---

## Validar duplicados: ¿nombre o SKU?

| | Nombre | SKU |
|---|---|---|
| ¿Debe ser único? | No necesariamente | Sí, es el identificador de negocio |
| Puede repetirse legítimamente | Sí (misma playera, distintas tallas) | No |
| Respaldo en la BD | No hay | Índice único: `HasIndex(p => p.SKU).IsUnique()` |

Validar duplicados con `ProductExists` está bien para dar un mensaje amable, pero **la garantía real es el índice único**. Si dos requests llegan al mismo tiempo, ambos pasan el `Exists` y solo el índice evita el duplicado.

---

## Revisión inicial de `ProductRepository` (pendientes)

| # | Pendiente | Prioridad |
|---|---|---|
| 1 | `Save()` lanza `NotImplementedException`; `BuyProduct`, `CreateProduct` y `DeleteProduct` dependen de él | 🔴 |
| 2 | `IProductRepository` no está registrado en `Program.cs` | 🔴 |
| 3 | `Product.Category` es `required`, pero `CreateProductDto` solo trae `CategoryId`; AutoMapper no puede llenarla | 🔴 |
| 4 | `BuyProduct`: `ToLower().Trim()` en la consulta; mejor comprar por `ProductId` o `SKU` | 🟠 |
| 5 | `BuyProduct`: leer, validar y restar no es atómico (condición de carrera). Opciones: `RowVersion` o `ExecuteUpdate` con `Where Stock >= qty` | 🟠 |
| 6 | `_db.Products.Update(product)` sobra: el producto ya está trackeado y `Update` marca **todas** las columnas | 🟠 |
| 7 | Lecturas sin `AsNoTracking()`, síncronas, sin paginación, con `ICollection` | 🟠 |
| 8 | `DateTime.Now` en lugar de `DateTime.UtcNow`; `UpdateDate = Now` al crear contradice que `null` signifique "nunca actualizado" | 🟡 |
| 9 | `SKU` sin índice único | 🟡 |

### `Save()` con `>= 0` siempre es `true`

```csharp
return _db.SaveChanges() >= 0 ? true : false;   // CategoryRepository
```

`SaveChanges()` devuelve el número de filas afectadas, que nunca es negativo. Esa condición **siempre** es `true`, incluso si se guardaron 0 filas. Un fallo real lanza excepción, pero la comparación está mal. Lo correcto según la intención es `> 0`.
