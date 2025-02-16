﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASP.NET_Core_MVC_Playground.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace ASP.NET_Core_MVC_Playground.Data
{
    public class SeedData
    {
        private readonly DataDbContext _db;
        private readonly ILogger _logger;
        private readonly Helpers helpers;

        public SeedData(DataDbContext db, ILogger<SeedData> logger, Helpers Helpers)
        {
            _db = db;
            _logger = logger;
            helpers = Helpers;
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            populateSellers();
            populateItems();
            populateBuyers();
            populateItemPurchases();
        }

        private void populateSellers()
        {
            if (_db.Sellers.Any())
            {
                _logger.LogInformation("Sellers already seeded!");
                return;
            }

            _db.Sellers.AddRange(
                    new Seller
                    {
                        FirstName = "George",
                        LastName = "Iniatis",
                        PhoneNumber = 99355566,
                        Email = "jogeo98@hotmail.com"
                    },
                    new Seller
                    {
                        FirstName = "Bill",
                        LastName = "Adama",
                        PhoneNumber = 99643155,
                        Email = "antonis@summecliff.com"
                    }
                );

            _db.SaveChanges();
            _logger.LogInformation("Sellers seeded!");
        }

        private void populateItems()
        {
            if (_db.Items.Any())
            {
                _logger.LogInformation("Items already seeded!");
                return;
            }

            List<Item> itemsList = new()
            {
                new Item
                {
                    Name = "Xbox",
                    Price = 400,
                    SellerId = (from sellers in _db.Sellers
                                where sellers.FullName == "George Iniatis"
                                select sellers.Id).First(),
                    Description = "Xbox Series X Console",
                    StripeImageUrl = "https://img-prod-cms-rt-microsoft-com.akamaized.net/cms/api/am/imageFileData/RE4mRni?ver=a707"
                },
                new Item
                {
                    Name = "Phone",
                    Price = 600,
                    SellerId = (from sellers in _db.Sellers
                                where sellers.FullName == "George Iniatis"
                                select sellers.Id).First(),
                    Description = "Galaxy S21 5G Smartphone",
                    StripeImageUrl = "https://www.mytrendyphone.eu/images/Samsung-Galaxy-S21-5G-128GB-Phantom-Grey-8806090892776-18012021-01-p.jpg"
                },
                new Item
                {
                    Name = "Battlestar Galactica",
                    Price = 15000,
                    SellerId = (from sellers in _db.Sellers
                                where sellers.FullName == "Bill Adama"
                                select sellers.Id).First(),
                    Description = "The pride of the colonial fleet",
                    StripeImageUrl = "https://rpggamer.org/uploaded_images/Bsg_damaged.jpg"
                },
                new Item
                {
                    Name = "Battlestar Pegasus",
                    Price = 30000,
                    SellerId = (from sellers in _db.Sellers
                                where sellers.FullName == "Bill Adama"
                                select sellers.Id).First(),
                    Description = "Improved battlestar but lacking in leadership",
                    StripeImageUrl = "https://static.wikia.nocookie.net/galactica/images/3/30/BattlestarPegasusPegasus.png/revision/latest?cb=20220308163308"
                }
            };

            // Create Stripe Products
            foreach(Item item in itemsList)
            {
                string[] newProductCreated = helpers.createStripeProduct(item);
                item.StripeId = newProductCreated[0];
                item.StripePriceId = newProductCreated[1];
                helpers.saveItem(item);
            }
            _logger.LogInformation("Items seeded!");
        }

        private void populateBuyers()
        {
            if (_db.Buyers.Any())
            {
                _logger.LogInformation("Buyers already seeded!");
                return;
            }

            _db.Buyers.AddRange(
                new Buyer
                {
                    FirstName = "Saul",
                    LastName = "Tigh"
                },
                new Buyer
                {
                    FirstName = "Helena",
                    LastName = "Cain"
                }
            );
            _db.SaveChanges();
            _logger.LogInformation("Buyers seeded!");
        }

        private void populateItemPurchases()
        {
            var anyBoughtItems = (from boughItems in _db.BoughtItems
                                  select boughItems).Any();
            if (!anyBoughtItems)
            {
                Item item = (from items in _db.Items
                             where items.Name == "Battlestar Galactica"
                             select items).First();

                Buyer buyer = (from buyers in _db.Buyers
                               where buyers.FullName == "Saul Tigh"
                               select buyers).First();

                BoughtItem newBoughtItem = new()
                {
                    Buyer = buyer,
                    BuyerId = buyer.Id,
                    Item = item,
                    ItemId = item.Id,
                    DateBought = DateTime.Now
                };

                _db.BoughtItems.Add(newBoughtItem);

                Item item2 = (from items in _db.Items
                              where items.Name == "Battlestar Pegasus"
                              select items).First();

                Buyer buyer2 = (from buyers in _db.Buyers
                                where buyers.FullName == "Helena Cain"
                                select buyers).First();

                BoughtItem newBoughtItem2 = new()
                {
                    Buyer = buyer2,
                    BuyerId = buyer2.Id,
                    Item = item2,
                    ItemId = item2.Id,
                    DateBought = DateTime.Now
                };

                _db.BoughtItems.Add(newBoughtItem2);

                _db.SaveChanges();
                _logger.LogInformation("Bought Items seeded!");

            }
        }
    }
}
