# Notas de conceptos y convenciones

Esta carpeta **no es documentación oficial del proyecto**. Son las **notas que tomamos para entender** los conceptos, convenciones y decisiones que van surgiendo mientras se construye `api-ecommerce`: dudas que salieron, cómo se resolvieron, comparaciones, analogías, ejemplos reales y errores que sirven para no repetirlos.

Se actualizan conforme avanza el curso. Para el README del proyecto, ver el de la raíz del repositorio.

## Cómo están organizadas

Una nota por **área**, no por duda. Las aclaraciones puntuales van dentro de la nota del área que les toca.

| Nota | De qué trata |
|---|---|
| [`efcore-fundamentos-migraciones.md`](efcore-fundamentos-migraciones.md) | Fundamentos de EF Core y migraciones |
| [`linq-materializar-consultas.md`](linq-materializar-consultas.md) | Ejecución diferida, materializar consultas, `IQueryable` vs `IEnumerable` vs colecciones |
| [`repositorios-consultas.md`](repositorios-consultas.md) | `Get` / `Search` / `Exists`, qué devolver y por qué, índices, pendientes del `ProductRepository` |
| [`aspnet-controllers.md`](aspnet-controllers.md) | Controllers: `IActionResult`, `[ProducesResponseType]`, rutas, `Name`, constraints, PATCH vs PUT |
| [`experimentos-rutas-swagger.md`](experimentos-rutas-swagger.md) | Bitácora de pruebas reales con rutas y Swagger, y qué falló en cada una |
| [`codigos-http-api.md`](codigos-http-api.md) | Códigos HTTP: cómo deducir cuáles devuelve un endpoint y diferencias entre ellos |
| [`dtos-automapper.md`](dtos-automapper.md) | DTOs vs entidades y AutoMapper |
| [`configuracion-proyecto.md`](configuracion-proyecto.md) | `launchSettings.json`, HTTPS y Swagger |
| [`convenciones-csharp-git.md`](convenciones-csharp-git.md) | Convenciones de nombres, namespaces y modificadores de acceso |

## Convenciones de las notas

- Cada concepto lleva, cuando aplica: analogía, tabla comparativa, ejemplo real del proyecto y cuándo usarlo.
- `⚠️` marca errores reales cometidos o riesgos que no dan error visible.
- Si algo está deducido y no probado, se indica como **por confirmar**.
- Si una nota ya cubre un tema, se amplía en lugar de crear otra.
