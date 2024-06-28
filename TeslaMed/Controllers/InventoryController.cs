using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeslaMed.Models;
using TeslaMed.ViewModels;

namespace TeslaMed.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        public readonly TeslaMedContext _context;
        private readonly IStringLocalizer<InventoryController> _localizer;
        public InventoryController(TeslaMedContext context, IStringLocalizer<InventoryController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }
        public async Task<IActionResult> ShowInventoryMainPage()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ShowAllRevisions(DateTime? filterDate, string nameFilter)
        {
            var query = _context.Revisions
                .AsQueryable();
            if (filterDate.HasValue)
            {
                query = query.Where(e => e.Date.Date == filterDate.Value.Date);
            }
            if (!string.IsNullOrEmpty(nameFilter))
            {
                query = query.Where(e => e.Name.ToLower().Contains(nameFilter.ToLower()));
            }
            var revisions = query.OrderByDescending(e => e.Date).ToList();
            return View(revisions);
        }



        [HttpGet]
        public async Task<IActionResult> CreateRevision()
        {
            ViewBag.Inventories = _context.Inventories.Include(i => i.InventoryName).ToList();
            ViewBag.InventoryNames = _context.InventoryNames.ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateRevision(RevisionViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var selectedInventory = _context.Inventories
                    .Include(i => i.InventoryName)
                    .FirstOrDefault(i => i.Id == viewModel.InventoryId);
                if (selectedInventory != null)
                {
                    var revision = new Revision
                    {
                        Date = DateTime.UtcNow,
                        Name = viewModel.Name,
                        Unit = viewModel.Unit,
                        ActualRemainder = viewModel.ActualRemainder,
                        SystemRemainder = selectedInventory.TotalAmount,
                        Variance = Math.Abs(viewModel.ActualRemainder - selectedInventory.TotalAmount),
                        InventoryId = viewModel.InventoryId,
                        InventoryNameId = viewModel.InventoryNameId
                    };

                    _context.Revisions.Add(revision);
                    _context.SaveChanges();

                    return RedirectToAction("ShowAllRevisions");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, _localizer["RevisionError"]);
                ViewBag.Inventories = _context.Inventories.Include(i => i.InventoryName).ToList();
                ViewBag.InventoryNames = _context.InventoryNames.ToList();
                return View(viewModel);
            }

            ViewBag.Inventories = _context.Inventories.Include(i => i.InventoryName).ToList();
            ViewBag.InventoryNames = _context.InventoryNames.ToList();
            return View(viewModel);
        }

        public async Task<IActionResult> Index()
        {
            var inventories = _context.Inventories.Include(i => i.InventoryName).ToList();
            return View(inventories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            var inventoryNames = _context.InventoryNames.ToList();
            ViewBag.InventoryNames = inventoryNames;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Inventory newInventory)
        {
            var inventoryNames = _context.InventoryNames.ToList();
            ViewBag.InventoryNames = inventoryNames;
            if (ModelState.IsValid)
            {
                var inventory = await _context.Inventories.AsNoTracking().FirstOrDefaultAsync(i => i.InventoryNameId == newInventory.InventoryNameId);
                if (inventory != null)
                {
                    ModelState.AddModelError("", _localizer["DuplicateError"]);
                    return View(newInventory);
                }
                newInventory.DateOfCreation = DateTime.Now;
                await _context.Inventories.AddAsync(newInventory);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(newInventory);
        }

        //Supply functionality
        public async Task<IActionResult> IndexSupply()
        {
            var supllies = _context.Supplies.Include(i => i.Inventory.InventoryName).ToList();
            return View(supllies);
        }
        [HttpPost]
        public async Task<IActionResult> IndexSupply(DateTime dateFilter, string nameFilter)
        {
            IQueryable<Supply> supplies = _context.Supplies.Include(s => s.Inventory).ThenInclude(i => i.InventoryName);
            if (dateFilter != default)
                supplies = supplies.Where(s => s.DateOfCreation.Date == dateFilter.Date);
            if (nameFilter != null)
                supplies = supplies.Where(s => s.Inventory.InventoryName.Name.ToLower().Contains(nameFilter.ToLower()));
            ViewBag.Filtering = true;
            return View(supplies.ToList());
        }
        public IActionResult CreateSupply()
        {
            var inventories = _context.Inventories.Include(i => i.InventoryName).ToList();
            ViewBag.Inventories = inventories;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateSupply(Supply newSupply)
        {
            var inventories = _context.Inventories.Include(i => i.InventoryName).ToList();
            ViewBag.Inventories = inventories;
            if (ModelState.IsValid)
            {
                var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == newSupply.InventoryId);
                if (inventory == null)
                    return NotFound();
                newSupply.DateOfCreation = DateTime.Now;
                inventory.TotalAmount += newSupply.SupplyQuantity;
                newSupply.TotalAmountWithSupply = inventory.TotalAmount;
                _context.Update(inventory);
                await _context.Supplies.AddAsync(newSupply);
                await _context.SaveChangesAsync();
                return RedirectToAction("IndexSupply");
            }
            return View(newSupply);
        }
        
        [HttpGet]
        public async Task<IActionResult> CurrentAccount()
        {
            var currentAccount = await _context.FlowAccountings.Include(f => f.User).Include(f => f.Inventory).ThenInclude(f => f.InventoryName).ToListAsync();
            return View(currentAccount);
        }

        [HttpPost]
        public async Task<IActionResult> CurrentAccount(DateTime dateFilter, string nameFilter)
        {
            IQueryable<FlowAccounting> currentAccount = _context.FlowAccountings.Include(f => f.User).Include(f => f.Inventory).ThenInclude(f => f.InventoryName);
            if (dateFilter != default)
                currentAccount = currentAccount.Where(c => c.Date.Date == dateFilter.Date);
            if (nameFilter != null)
                currentAccount = currentAccount.Where(c => c.Inventory.InventoryName.Name.ToLower().Contains(nameFilter.ToLower()));
            ViewBag.Filtering = true;
            return View(currentAccount.ToList());
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CurrentAccountCreate()
        {
            ViewBag.Inventories = await _context.Inventories.Include(i => i.InventoryName).ToListAsync();
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CurrentAccountCreate(FlowAccounting accounting)
        {
            if (ModelState.IsValid)
            {
                accounting.Date = DateTime.Now;
                accounting.User = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == accounting.InventoryId);
                if (inventory == null)
                    return BadRequest();
                inventory.TotalAmount -= accounting.Quantity;
                accounting.InventoryTotalBalance = inventory.TotalAmount;
                await _context.AddAsync(accounting);
                _context.Update(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction("CurrentAccount");
            }
            ModelState.AddModelError("", _localizer["CurrentAccountError"]);
            ViewBag.Inventories = await _context.Inventories.Include(i => i.InventoryName).ToListAsync();
            return View(accounting);
        }
    }
}
