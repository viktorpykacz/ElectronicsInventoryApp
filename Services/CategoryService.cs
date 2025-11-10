using ElectronicsInventoryApp.Models;
using ElectronicsInventoryApp.Services;
using System.Collections.Generic;
using System.Linq;

public class CategoryService
{
    private readonly JsonRepository<Category> _repo;
    private List<Category> _cache;

    public CategoryService(string filePath)
    {
        _repo = new JsonRepository<Category>(filePath);
        _cache = _repo.Load() ?? new List<Category>();
    }

    /// <summary>
    /// Zwraca wszystkie kategorie g³ówne (czyli te bez rodzica).
    /// Ka¿da z nich mo¿e mieæ zagnie¿d¿one Subcategories.
    /// </summary>
    public IEnumerable<Category> GetAll()
    {
        return _cache.OrderBy(c => c.Name);
    }

    /// <summary>
    /// Zwraca wszystkie kategorie w postaci sp³aszczonej listy (wraz z podkategoriami).
    /// </summary>
    public IEnumerable<Category> GetAllFlattened()
    {
        var result = new List<Category>();
        foreach (var root in _cache)
            Flatten(root, result);
        return result.OrderBy(c => c.Name);
    }

    /// <summary>
    /// Wyszukuje kategoriê po jej identyfikatorze (tak¿e w podkategoriach).
    /// </summary>
    public Category? GetById(int id)
    {
        return GetAllFlattened().FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Dodaje now¹ kategoriê lub podkategoriê.
    /// Jeœli parentId != null, dodaje jako dziecko istniej¹cej kategorii.
    /// </summary>
    public void Add(Category category, int? parentId = null)
    {
        if (parentId == null)
        {
            _cache.Add(category);
        }
        else
        {
            var parent = GetById(parentId.Value);
            if (parent != null)
            {
                parent.Subcategories.Add(category);
                category.ParentId = parent.Id;
            }
        }

        _repo.Save(_cache);
    }

    /// <summary>
    /// Usuwa kategoriê o podanym Id (rekurencyjnie w ca³ym drzewie).
    /// </summary>
    public void Delete(int id)
    {
        if (TryDeleteFromList(_cache, id))
            _repo.Save(_cache);
    }

    private bool TryDeleteFromList(List<Category> list, int id)
    {
        var item = list.FirstOrDefault(c => c.Id == id);
        if (item != null)
        {
            list.Remove(item);
            return true;
        }

        foreach (var cat in list)
        {
            if (TryDeleteFromList(cat.Subcategories, id))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Zastêpuje dane kategorii (aktualizacja).
    /// </summary>
    public void Update(Category category)
    {
        var existing = GetById(category.Id);
        if (existing == null) return;

        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.ParentId = category.ParentId;
        _repo.Save(_cache);
    }

    // --- Pomocnicze ---
    private void Flatten(Category node, List<Category> result)
    {
        result.Add(node);
        foreach (var sub in node.Subcategories)
            Flatten(sub, result);
    }

    public string GetFullPath(int categoryId)
    {
        var parts = new List<string>();
        var current = GetById(categoryId);

        while (current != null)
        {
            parts.Insert(0, current.Name);
            current = current.ParentId.HasValue ? GetById(current.ParentId.Value) : null;
        }

        return string.Join(" / ", parts);
    }

    /// <summary>
    /// Nadpisuje wszystkie kategorie now¹ list¹ (np. po imporcie JSON-a).
    /// </summary>
    public void SetAll(List<Category> categories)
    {
        _cache = categories ?? new List<Category>();
        _repo.Save(_cache);
    }

    public (string? Category, string? Subcategory, string? Type) GetPathLevels(int categoryId)
    {
        var path = new List<Category>();
        var current = GetById(categoryId);

        while (current != null)
        {
            path.Insert(0, current);
            if (current.ParentId == null) break;
            current = GetById(current.ParentId.Value);
        }

        // Uzupe³niamy do 3 poziomów
        string? l1 = path.Count > 0 ? path[0].Name : null;
        string? l2 = path.Count > 1 ? path[1].Name : null;
        string? l3 = path.Count > 2 ? path[2].Name : null;

        return (l1, l2, l3);
    }
}
