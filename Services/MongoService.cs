using MongoDB.Driver;
using AtlasViewer.Models;

namespace AtlasViewer.Services
{
 public class MongoService
 {
 private readonly IMongoDatabase _database;
 private readonly IMongoCollection<Item> _items;

 public MongoService(IMongoDatabase database)
 {
 _database = database;
 _items = _database.GetCollection<Item>("items");
 }

 public async Task<List<Item>> GetItemsAsync()
 {
 return await _items.Find(FilterDefinition<Item>.Empty).ToListAsync();
 }
 }
}
