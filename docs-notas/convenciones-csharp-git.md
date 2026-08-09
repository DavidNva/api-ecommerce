# Convenciones — Nombres en GitHub y Namespaces en C#

## Nombres de repositorios en GitHub

**Estándar:** `kebab-case`, todo en minúsculas.

```
api-ecommerce         ✅
ApiEcommerce           ❌
api_ecommerce          ❌ (evitar guion bajo)
```

No es una norma oficial (no hay ISO/RFC que lo imponga) — es una convención que ganó por practicidad técnica y se reforzó por repetición masiva en la comunidad (Microsoft, Google, Facebook, etc. la siguen).

**Por qué conviene:**
- Las URLs son parte de la identidad del repo (`github.com/user/mi-repo`) — minúsculas + guiones evita problemas de codificación.
- **Unix/Linux distingue mayúsculas de minúsculas** en el sistema de archivos; Windows/Mac no. Un repo `Repo` y otro `repo` son cosas distintas en Linux → bugs raros al clonar en distintos SO.

| Elemento | Convención | Ejemplo |
|---|---|---|
| Nombre de repo | kebab-case minúsculas | `api-ecommerce` |
| Branches | kebab-case + prefijo | `feature/add-login-endpoint` |
| Archivos `.cs` | PascalCase (regla de C#) | `CategoryRepository.cs` |

---

## Namespaces en C#

### Los dos estilos válidos

**Con llaves (block-scoped)** — el que genera Visual Studio por default:
```csharp
namespace api_ecommerce.Models
{
    public class Category
    {
    }
}
```

**File-scoped (una línea, sin llaves)** — disponible desde C# 10 / .NET 6+, más limpio:
```csharp
namespace api_ecommerce.Models;

public class Category
{
}
```

### ¿Y si no hay namespace declarado?

```csharp
public class Category
{
}
```

Esto compila, pero la clase queda en el **namespace global implícito** — evitarlo en proyectos reales:
- Riesgo de colisión de nombres con otras clases/librerías.
- Pierdes la organización lógica (`Models`, `Services`, `Controllers`) que da el namespace.
- No es lo que vas a ver en proyectos profesionales.

**Regla:** siempre declara namespace, y que coincida con la carpeta donde vive el archivo (`Models/` → `.Models`, `Controllers/` → `.Controllers`).

### Cómo forzar file-scoped en Visual Studio

Agregar a `.editorconfig`:
```ini
csharp_style_namespace_declarations = file_scoped:suggestion
```
