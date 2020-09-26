﻿using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using System.Runtime.Serialization;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
    public class BasketService : IBasketService
    {
        private readonly IAsyncRepository<Basket> _basketRepository;
        private readonly IAppLogger<BasketService> _logger;

        public BasketService(IAsyncRepository<Basket> basketRepository,
            IAppLogger<BasketService> logger)
        {
            _basketRepository = basketRepository;
            _logger = logger;
        }

        public async Task AddItemToBasket(int basketId, int catalogItemId, decimal price, int quantity = 1)
        {
            var basketSpec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);
            Guard.Against.NullBasket(basketId, basket);

            basket.AddItem(catalogItemId, price, quantity);

            await _basketRepository.UpdateAsync(basket);
        }

        public async Task DeleteBasketAsync(int basketId)
        {
            var basket = await _basketRepository.GetByIdAsync(basketId);
            await _basketRepository.DeleteAsync(basket);
        }

        public async Task SetQuantities(int basketId, Dictionary<string, int> quantities)
        {
            Guard.Against.Null(quantities, nameof(quantities));
            var basketSpec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);
            Guard.Against.NullBasket(basketId, basket);

            foreach (var item in basket.Items)
            {
                if (quantities.TryGetValue(item.Id.ToString(), out var quantity))
                {
                    if (_logger != null) _logger.LogInformation($"Updating quantity of item ID:{item.Id} to {quantity}.");
                    item.SetQuantity(quantity);

                    if(quantity >= 5)
                    {
                        throw new OpsNoMoreThanFourItems($"No more than 4 items of {item.Id} allowed");
                    }
                }
            }
            basket.RemoveEmptyItems();
            await _basketRepository.UpdateAsync(basket);
        }

        public async Task TransferBasketAsync(string anonymousId, string userName)
        {
            Guard.Against.NullOrEmpty(anonymousId, nameof(anonymousId));
            Guard.Against.NullOrEmpty(userName, nameof(userName));
            var anonymousBasketSpec = new BasketWithItemsSpecification(anonymousId);
            var anonymousBasket = await _basketRepository.FirstOrDefaultAsync(anonymousBasketSpec);
            if (anonymousBasket == null) return;
            var userBasketSpec = new BasketWithItemsSpecification(userName);
            var userBasket = await _basketRepository.FirstOrDefaultAsync(userBasketSpec);
            if (userBasket == null)
            {
                userBasket = new Basket(userName);
                await _basketRepository.AddAsync(userBasket);
            }
            foreach (var item in anonymousBasket.Items)
            {
                userBasket.AddItem(item.CatalogItemId, item.UnitPrice, item.Quantity);
            }
            await _basketRepository.UpdateAsync(userBasket);
            await _basketRepository.DeleteAsync(anonymousBasket);
        }
    }

    [System.Serializable]
    internal class OpsNoMoreThanFourItems : System.Exception
    {
        public OpsNoMoreThanFourItems()
        {
        }

        public OpsNoMoreThanFourItems(string message) : base(message)
        {
        }

        public OpsNoMoreThanFourItems(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected OpsNoMoreThanFourItems(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
