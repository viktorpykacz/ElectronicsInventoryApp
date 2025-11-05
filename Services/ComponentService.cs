using ElectronicsInventoryApp.Models;
using ElectronicsInventoryApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class ComponentService
{
    private readonly JsonRepository<ElectronicPart> _repo;
    private List<ElectronicPart> _cache;

    public ComponentService(string filePath)
    {
        _repo = new JsonRepository<ElectronicPart>(filePath);
        _cache = _repo.Load() ?? new List<ElectronicPart>();
    }

    public IEnumerable<ElectronicPart> GetAll() => _cache.OrderBy(c => c.Name);

    public ElectronicPart? GetById(int id) => _cache.FirstOrDefault(c => c.Id == id);

    public void Add(ElectronicPart c)
    {
        c.Id = _cache.Any() ? _cache.Max(x => x.Id) + 1 : 1;
        c.CreatedAt = DateTime.UtcNow;
        c.UpdatedAt = DateTime.UtcNow;
        _cache.Add(c);
        _repo.Save(_cache);
    }

    public void Update(ElectronicPart c)
    {
        var idx = _cache.FindIndex(x => x.Id == c.Id);
        if (idx == -1)
            throw new Exception("Component not found");

        c.UpdatedAt = DateTime.UtcNow;
        _cache[idx] = c;
        _repo.Save(_cache);
    }

    public void Delete(int id)
    {
        _cache.RemoveAll(x => x.Id == id);
        _repo.Save(_cache);
    }

    public IEnumerable<ElectronicPart> Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return GetAll();

        q = q.Trim().ToLowerInvariant();

        return _cache.Where(c =>
            (c.Name ?? "").ToLowerInvariant().Contains(q) ||
            (c.PartNumber ?? "").ToLowerInvariant().Contains(q) ||
            (c.Description ?? "").ToLowerInvariant().Contains(q) ||
            (c.Manufacturer ?? "").ToLowerInvariant().Contains(q)
        ).OrderBy(c => c.Name);
    }

    /// <summary>
    /// Nadpisuje wszystkie komponenty nową listą (np. po imporcie JSON-a).
    /// </summary>
    public void SetAll(List<ElectronicPart> parts)
    {
        _cache = parts ?? new List<ElectronicPart>();
        _repo.Save(_cache);
    }
}
