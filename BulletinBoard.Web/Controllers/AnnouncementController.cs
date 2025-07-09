using BulletinBoard.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulletinBoard.Web.Controllers;

public class AnnouncementsController : Controller
{
    private readonly HttpClient _client;
    public AnnouncementsController(IHttpClientFactory factory) =>
        _client = factory.CreateClient("api");

    [HttpGet]
    public async Task<IActionResult> Index(string category, string subCategory)
    {
        var url = "announcements";
        var sep = "?";
        if (!string.IsNullOrEmpty(category))
        {
            url += $"{sep}category={Uri.EscapeDataString(category)}";
            sep = "&";
        }
        if (!string.IsNullOrEmpty(subCategory))
        {
            url += $"{sep}subCategory={Uri.EscapeDataString(subCategory)}";
        }

        var items = await _client.GetFromJsonAsync<List<Announcement>>(url);
        var subs = new Dictionary<string, string[]>
        {
            ["Household appliances"] = new[] { "Refrigerators", "Washing machines", "Boilers", "Ovens", "Hoods", "Microwaves" },
            ["Computer technology"] = new[] { "PC", "Laptops", "Monitors", "Printers", "Scanners" },
            ["Smartphones"] = new[] { "Android smartphones", "iOS/Apple smartphones" },
            ["Other"] = new[] { "Clothing", "Footwear", "Accessories", "Sports equipment", "Toys" }
        };

        ViewBag.CategoryList = new SelectList(subs.Keys, category);
        ViewBag.SubCategoryList = !string.IsNullOrEmpty(category) && subs.ContainsKey(category)
            ? new SelectList(subs[category], subCategory)
            : new SelectList(Enumerable.Empty<string>());

        ViewBag.SelectedCategory = category;
        ViewBag.SelectedSubCategory = subCategory;

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var ann = await _client.GetFromJsonAsync<Announcement>($"announcements/{id}");
        if (ann == null) return NotFound();
        return View(ann);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.CategoryList = new SelectList(new[]
        {
            "Household appliances",
            "Computer technology",
            "Smartphones",
            "Other"
        });
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Announcement model)
    {
        ViewBag.CategoryList = new SelectList(new[]
        {
            "Household appliances",
            "Computer technology",
            "Smartphones",
            "Other"
        }, model.Category);

        if (!ModelState.IsValid) return View(model);

        var resp = await _client.PostAsJsonAsync("announcements", model);
        if (resp.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
        ModelState.AddModelError("", "API error");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var item = await _client.GetFromJsonAsync<Announcement>($"announcements/{id}");
        if (item == null) return NotFound();
        ViewBag.CategoryList = new SelectList(new[]
        {
            "Household appliances",
            "Computer technology",
            "Smartphones",
            "Other"
        }, item.Category);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Announcement model)
    {
        ViewBag.CategoryList = new SelectList(new[]
        {
            "Household appliances",
            "Computer technology",
            "Smartphones",
            "Other"
        }, model.Category);

        if (!ModelState.IsValid) return View(model);

        var resp = await _client.PutAsJsonAsync($"announcements/{model.Id}", model);
        if (resp.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
        ModelState.AddModelError("", "API error");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _client.DeleteAsync($"announcements/{id}");
        return RedirectToAction(nameof(Index));
    }
}
