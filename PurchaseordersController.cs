using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using ERP_API.Data;
using ERP_API.Moduls;

namespace ERP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseordersController : ControllerBase
    {
        private readonly Lg202324Context _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseordersController> _logger;

        public PurchaseordersController(Lg202324Context context, IMapper mapper, ILogger<PurchaseordersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Purchaseorders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseorderReadOnly>>> GetPurchaseorders()
        {
            try
            {
                if (_context.Purchaseorders == null)
                {
                    _logger.LogWarning("Attempted to retrieve purchase orders, but the entity set was null.");
                    return NotFound();
                }

                var purchaseOrders = await _context.Purchaseorders
                    .Include(p => p.PurchaseorderSubs)
                    .OrderByDescending(p => p.POId)
                    .ToListAsync();

                var purchaseOrderDtos = _mapper.Map<IEnumerable<PurchaseorderReadOnly>>(purchaseOrders);

                _logger.LogInformation("Retrieved all purchase orders with their subs.");
                return Ok(purchaseOrderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving purchase orders.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // GET: api/Purchaseorders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseorderReadOnly>> GetPurchaseorder(int id)
        {
            try
            {
                var purchaseOrder = await _context.Purchaseorders
                    .Include(p => p.PurchaseorderSubs)
                    .FirstOrDefaultAsync(p => p.POId == id);

                if (purchaseOrder == null)
                {
                    _logger.LogWarning($"Purchase order with ID {id} was not found.");
                    return NotFound();
                }

                var purchaseOrderDto = _mapper.Map<PurchaseorderReadOnly>(purchaseOrder);

                _logger.LogInformation($"Retrieved purchase order with ID {id} and its subs.");
                return Ok(purchaseOrderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving purchase order with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // POST: api/Purchaseorders
        [HttpPost]
        public async Task<ActionResult<PurchaseorderReadOnly>> PostPurchaseorder(PurchaseorderCreateOnly purchaseOrderDto)
        {
            if (_context.Purchaseorders == null)
            {
                _logger.LogWarning("Attempted to create a purchase order but the entity set was null.");
                return Problem("Entity set 'Lg202324Context.Purchaseorders' is null.");
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Map the incoming DTO to the entity
                    var purchaseOrder = _mapper.Map<Purchaseorder>(purchaseOrderDto);

                    // Add the purchase order to the context
                    _context.Purchaseorders.Add(purchaseOrder);
                    await _context.SaveChangesAsync();

                    // Set the POId for all subs
                    if (purchaseOrder.PurchaseorderSubs != null)
                    {
                        foreach (var sub in purchaseOrder.PurchaseorderSubs)
                        {
                            sub.POId = purchaseOrder.POId;
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    // Retrieve the created purchase order with subs
                    var createdOrder = await _context.Purchaseorders
                        .Include(p => p.PurchaseorderSubs)
                        .FirstOrDefaultAsync(p => p.POId == purchaseOrder.POId);

                    var createdOrderDto = _mapper.Map<PurchaseorderReadOnly>(createdOrder);

                    _logger.LogInformation($"Created a new purchase order with ID {purchaseOrder.POId}.");
                    return CreatedAtAction(nameof(GetPurchaseorder), new { id = purchaseOrder.POId }, createdOrderDto);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a purchase order.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // PUT: api/Purchaseorders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPurchaseorder(int id, PurchaseorderCreateOnly purchaseOrderDto)
        {
            if (id != purchaseOrderDto.POId)
            {
                _logger.LogWarning("PUT request failed due to mismatched IDs.");
                return BadRequest();
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var existingOrder = await _context.Purchaseorders
                        .Include(p => p.PurchaseorderSubs)
                        .FirstOrDefaultAsync(p => p.POId == id);

                    if (existingOrder == null)
                    {
                        _logger.LogWarning($"Purchase order with ID {id} does not exist.");
                        return NotFound();
                    }

                    // Remove existing subs
                    _context.PurchaseorderSubs.RemoveRange(existingOrder.PurchaseorderSubs);

                    // Update main entity
                    _mapper.Map(purchaseOrderDto, existingOrder);

                    // Ensure all new subs have the correct POId
                    if (existingOrder.PurchaseorderSubs != null)
                    {
                        foreach (var sub in existingOrder.PurchaseorderSubs)
                        {
                            sub.POId = id;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Updated purchase order with ID {id}.");
                    return NoContent();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating purchase order with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // DELETE: api/Purchaseorders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseorder(int id)
        {
            try
            {
                var purchaseOrder = await _context.Purchaseorders
                    .Include(p => p.PurchaseorderSubs)
                    .FirstOrDefaultAsync(p => p.POId == id);

                if (purchaseOrder == null)
                {
                    _logger.LogWarning($"Purchase order with ID {id} was not found.");
                    return NotFound();
                }

                _context.Purchaseorders.Remove(purchaseOrder);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted purchase order with ID {id}.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting purchase order with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        private bool PurchaseorderExists(int id)
        {
            return (_context.Purchaseorders?.Any(e => e.POId == id)).GetValueOrDefault();
        }
    }
}