using api_ecommerce.Models;
using api_ecommerce.Models.Dtos;
using api_ecommerce.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace api_ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryRepository categoryRepository,
        IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]//Se procesó con éxito y se devolvió el recurso solicitado
        [ProducesResponseType(StatusCodes.Status403Forbidden)]//Sin permisos
        public IActionResult GetCategories()
        {
            var categories = _categoryRepository.GetCategories();
            var categoriesDto = new List<CategoryDto>();

            foreach (var category in categories)
            {
                categoriesDto.Add(_mapper.Map<CategoryDto>(category));
            }

            return Ok(categoriesDto);
        }

        [HttpGet("{id:int}", Name = "GetCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]//Se procesó con éxito y se devolvió el recurso solicitado
        [ProducesResponseType(StatusCodes.Status403Forbidden)]//Sin permisos
        [ProducesResponseType(StatusCodes.Status400BadRequest)]//Mal formada la solicitud
        [ProducesResponseType(StatusCodes.Status404NotFound)]//Recurso no encontrado
        public IActionResult GetCategory(int id)
        {
            var category = _categoryRepository.GetCategory(id);
            if (category == null)
            {
                return NotFound($"La categoria con id {id} no existe");
            }
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]//Se procesó con éxito y se creó un nuevo recurso
        [ProducesResponseType(StatusCodes.Status403Forbidden)]//Sin permisos
        [ProducesResponseType(StatusCodes.Status400BadRequest)]//Mal formada la solicitud
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]//Aplica, en autorizacion, si el usuario no esta autenticado o autorizado en estecaso
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]//Error del servidor
        public IActionResult CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (createCategoryDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_categoryRepository.CategoryExists(createCategoryDto.Name))
            {
                ModelState.AddModelError("CustomError", "La categoría ya existe");
                return BadRequest(ModelState);
            }

            var category = _mapper.Map<Category>(createCategoryDto);

            if (!_categoryRepository.CreateCategory(category))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al guardar la categoría {category.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
        }

        [HttpPatch("{id:int}", Name = "UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]//Se procesó con éxito, pero no hay contenido que devolver
        [ProducesResponseType(StatusCodes.Status403Forbidden)]//Sin permisos
        [ProducesResponseType(StatusCodes.Status400BadRequest)]//Mal formada la solicitud
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]//Aplica, en autorizacion, si el usuario no esta autenticado o autorizado en estecaso
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]//Error del servidor
        public IActionResult UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
        {
            if (!_categoryRepository.CategoryExists(id))
            {
                return NotFound($"La categoria con id {id} no existe");
            }

            if (updateCategoryDto == null)
            {
                return BadRequest(ModelState);
            }

            if (_categoryRepository.CategoryExists(updateCategoryDto.Name))
            {
                ModelState.AddModelError("CustomError", "La categoría ya existe");
                return BadRequest(ModelState);
            }

            var category = _mapper.Map<Category>(updateCategoryDto);
            category.Id = id;

            if (!_categoryRepository.UpdateCategory(category))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al actualizar la categoría {category.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]//Se procesó con éxito, pero no hay contenido que devolver
        [ProducesResponseType(StatusCodes.Status403Forbidden)]//Sin permisos
        [ProducesResponseType(StatusCodes.Status400BadRequest)]//Mal formada la solicitud
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]//Aplica, en autorizacion, si el usuario no esta autenticado o autorizado en estecaso
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]//Error del servidor
        public IActionResult DeleteCategory(int id)
        {
            var category = _categoryRepository.GetCategory(id);

            if (category == null)
            {
                return NotFound($"La categoria con id {id} no existe");
            }

            if (!_categoryRepository.DeleteCategory(category))
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al eliminar la categoría {category.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}