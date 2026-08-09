using api_ecommerce.Models;

namespace api_ecommerce.Repository;

public interface ICategoryRepository
{
    ICollection<Category> GetCategories();
    Category GetCategory(int id);
    bool CategoryExists(int id);
    bool CategoriesExists(string name);

    bool CreateCategory(Category category);
    bool UpdateCategory(Category category);
    bool DeleteCategory(Category category);
    bool Save();
}
