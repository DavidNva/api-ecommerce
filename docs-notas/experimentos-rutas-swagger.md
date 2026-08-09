# Experimentos — Rutas, Constraints y Swagger

Bitácora de pruebas reales hechas sobre `GetCategory`/`CreateCategory`, probando distintas combinaciones de `Name`, constraints de ruta, y parámetros del método. Para la explicación conceptual de cada mecanismo, ver `aspnet-controllers.md`.

---

## Prueba 1 — `Name` distinto entre el GET y el `CreatedAtRoute` del POST

```csharp
[HttpGet("{id:int}", Name = "GetCategory1")] // se cambió el Name
public IActionResult GetCategory(int id) { ... }
```
```csharp
// El POST sigue buscando el nombre viejo:
return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
```

| Acción | Resultado |
|---|---|
| `GET /api/categories/1` desde el navegador | ✅ Funciona normal — el `Name` no interviene en el enrutamiento directo |
| `POST /api/Categories` (crear categoría) | ❌ `500 InternalServerError` — `System.InvalidOperationException: No route matches the supplied values` |

**Confirma:** la categoría **sí se crea** en la BD (el `SaveChanges()` ya se ejecutó) antes de que truene — el error ocurre después, al intentar construir la respuesta con `CreatedAtRoute`.

---

## Prueba 2 — Mismo `Name` en dos rutas con templates distintos

```csharp
[HttpGet(Name = "GetCategory")]           // template: api/Categories
public IActionResult GetCategories() { ... }

[HttpGet("{id:int}", Name = "GetCategory")] // template: api/Categories/{id} — mismo Name
public IActionResult GetCategory(int id) { ... }
```

**Resultado:** la aplicación **no arranca**:
```
InvalidOperationException: Attribute routes with the same name 'GetCategory' must have the same template
```

**Regla exacta:** se puede repetir el mismo `Name` en varios endpoints solo si **todos** tienen el mismo template de ruta. Si los templates difieren, ASP.NET Core no puede resolver la ambigüedad y falla al iniciar (falla rápido y claro, en vez de dejar un bug ambiguo en producción).

---

## Prueba 3 — Constraint `{id:string}` (no existe)

```csharp
[HttpGet("{id:string}", Name = "GetCategory")]
```

**Resultado:** falla **al ejecutar** el proyecto (no al compilar):
```
InvalidOperationException: The constraint reference 'string' could not be resolved to a type.
Register the constraint type with 'RouteOptions.ConstraintMap'.
```

**Por qué:** `:string` no es un constraint válido en ASP.NET Core — no hace falta, porque sin ningún constraint la ruta ya acepta cualquier texto por default. Constraints válidos: `:int`, `:bool`, `:guid`, `:datetime`, `:alpha`, etc.

---

## Prueba 4 — Constraint `{id:bool}` con parámetro `int id` (tipos que no coinciden)

```csharp
[HttpGet("{id:bool}", Name = "GetCategory")]
public IActionResult GetCategory(int id) { ... }
```

| Request | Resultado |
|---|---|
| `GET /api/Categories/true` | El constraint `:bool` se cumple (matchea la ruta), pero el model binding falla al convertir `"true"` a `int` → **`400 Bad Request`**: `"The value 'true' is not valid."` |
| Swagger UI, intentar mandar `1` en el campo | Bloqueado **antes de enviar el request** — Swagger UI valida contra el constraint documentado (`boolean`) y no deja mandar un entero |

**Confirma:** hay dos validaciones en cascada — primero el constraint de la ruta, después la conversión al tipo real del parámetro del método. Ambas deben pasar para que el código se ejecute.

---

## Prueba 5 — Quitar `{id:int}` de la ruta, dejando `int id` en el método

```csharp
[HttpGet(Name = "GetCategory")] // sin {id:int}
public IActionResult GetCategory(int id) { ... }
```

**Con `GetCategories()` activo al mismo tiempo (mismo template `api/Categories`):**
```
Swagger: "Failed to load API definition" — 500 Internal Server Error en /swagger/v1/swagger.json
```
Causa real: ruta duplicada (mismo verbo + mismo template que `GetCategories`), no el `Name` en sí.

**Con `GetCategories()` comentado/ausente (sin conflicto):**
```
GET /api/Categories?id=4  →  200 OK
```
El parámetro `id` se resuelve automáticamente vía **query string**, y Swagger lo documenta como `(query)` en vez de `(path)`.

---

## Prueba 6 — Quitar el parámetro `id` del método, dejando `{id:int}` en la ruta

```csharp
[HttpGet("{id:int}", Name = "GetCategory")]
public IActionResult GetCategory() // sin parámetro id
{
    var id = 2; // hardcodeado
    var category = _categoryRepository.GetCategory(id);
    ...
}
```

**Resultado:** `GET /api/Categories/1` responde `200 OK`, pero **regresa la categoría con id=2**, ignorando el `1` de la URL.

**Confirma:** la ruta matchea perfectamente (define qué URL activa el método), pero el **model binding** solo conecta el valor de la URL a un parámetro si el método declara uno con el mismo nombre. Sin parámetro `id` en la firma, el valor de la URL se recibe pero no se usa — se pierde silenciosamente, sin ningún error.

---

## Prueba 7 — `405 Method Not Allowed` al navegar directo a una URL

Con `GetCategory` sin `{id:int}` (ver Prueba 5) pero con `UpdateCategory` usando `[HttpPatch("{id:int}")]` en la misma ruta base:

```
GET http://localhost:5000/api/Categories/3  →  405 Method Not Allowed
```

**Por qué 405 y no 404:** la ruta `/api/Categories/{id}` **sí existe** (matchea el template de `UpdateCategory`), pero solo acepta `PATCH`, no `GET` — de ahí que el servidor conteste "método no permitido" en vez de "no encontrado". Mismo comportamiento esperado en Postman o cualquier cliente HTTP real, no es un tema exclusivo del navegador.

---

## Resumen de errores según el momento en que aparecen

| Momento | Tipo de error |
|---|---|
| Al iniciar la app | `Name` duplicado con templates distintos · constraint inexistente (`:string`) |
| En runtime, solo al ejecutar esa línea | `CreatedAtRoute` busca un `Name` que no existe |
| En runtime, al recibir el request | Constraint no coincide con el tipo del parámetro (`400`) · ruta existe pero verbo no coincide (`405`) |
| Nunca falla, pero es un bug silencioso | Falta el parámetro en la firma del método (se ignora el valor de la URL) · quitar `{id:int}` cambia path→query sin que nadie lo note |
