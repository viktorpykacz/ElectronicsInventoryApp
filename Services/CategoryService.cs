using ElectronicsInventoryApp.Models;
using ElectronicsInventoryApp.Services;
using System.Collections.Generic;
using System.Linq;

public class CategoryService
{
    private readonly JsonRepository<Category> _repo;
    private readonly List<Category> _cache;

    public CategoryService(string filePath)
    {
        _repo = new JsonRepository<Category>(filePath);
        _cache = _repo.Load();
    }

    public IEnumerable<Category> GetAll() => _cache.OrderBy(c => c.Name);
    public Category? GetById(int id) => _cache.FirstOrDefault(c => c.Id == id);
    public void Add(Category c)
    {
        c.Id = _cache.Any() ? _cache.Max(x => x.Id) + 1 : 1;
        _cache.Add(c);
        _repo.Save(_cache);
    }
    public void Update(Category c)
    {
        var idx = _cache.FindIndex(x => x.Id == c.Id);
        if (idx == -1) throw new Exception("Component not found");
        _cache[idx] = c;
        _repo.Save(_cache);
    }
    public void Delete(int id)
    {
        _cache.RemoveAll(x => x.Id == id);
        _repo.Save(_cache);
    }
    public IEnumerable<Category> Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return GetAll();
        q = q.Trim().ToLowerInvariant();
        return _cache.Where(c =>
        (c.Name ?? "").ToLowerInvariant().Contains(q)
        ).OrderBy(c => c.Name);
    }
}
