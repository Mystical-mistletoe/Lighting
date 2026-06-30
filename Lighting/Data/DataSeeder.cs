using Lighting.Models;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Lighting.Data
{
    public static class DataSeeder
    {
        private class JsonItem
        {
            public string? code { get; set; }
            public string? article { get; set; }
            public string? name { get; set; }
            public decimal price { get; set; }
            public decimal remaining { get; set; }
            public string? catalog { get; set; }
            public List<JsonProperty>? properties { get; set; }
        }

        private class JsonProperty
        {
            public string? property { get; set; }
            public string? code { get; set; }
            public string? value { get; set; }
        }

        public static void Seed(AppDbContext db)
        {
            if (db.Categories.Any()) return;

            var random = new Random(42);

            // Категории 
            var categories = new List<Category>
            {
                new Category { Name = "Плафоны для внутр. освещения", Description = "Плафоны и абажуры для светильников" },
                new Category { Name = "Офисно-административные светильники", Description = "Светильники для офисов и административных помещений" },
                new Category { Name = "Люстры", Description = "Потолочные люстры для дома и интерьера" },
                new Category { Name = "Подвесные светильники", Description = "Подвесные светильники на шнуре или цепи" },
                new Category { Name = "Потолочные светильники", Description = "Накладные и встраиваемые потолочные светильники" },
                new Category { Name = "Бра", Description = "Настенные светильники бра" },
                new Category { Name = "Настольные лампы", Description = "Лампы для рабочего стола и декора" },
                new Category { Name = "Торшеры", Description = "Напольные светильники торшеры" },
                new Category { Name = "Споты", Description = "Поворотные споты и направленный свет" },
                new Category { Name = "Универсальные крепления", Description = "Крепежи, кронштейны и аксессуары для монтажа" },
                new Category { Name = "Встраиваемые светильники", Description = "Точечные встраиваемые светильники" },
                new Category { Name = "Трековые светильники и аксессуары к ним", Description = "Трековые системы и комплектующие" },
            };
            db.Categories.AddRange(categories);
            db.SaveChanges();

            // Производители (
            var manufacturers = new List<Manufacturer>
            {
                new Manufacturer { Name = "IEK", Country = "Россия", Website = "www.iek.ru" },
                new Manufacturer { Name = "Volpe", Country = "Россия", Website = "www.volpe.ru" },
                new Manufacturer { Name = "TDM Electric", Country = "Россия", Website = "www.tdm-electric.ru" },
                new Manufacturer { Name = "Uniel", Country = "Россия", Website = "www.uniel.ru" },
                new Manufacturer { Name = "Feron", Country = "Россия", Website = "www.feron.ru" },
                new Manufacturer { Name = "Gauss", Country = "Россия", Website = "www.gauss.ru" },
                new Manufacturer { Name = "General", Country = "Китай", Website = "www.general-led.com" },
                new Manufacturer { Name = "Световые Технологии", Country = "Россия", Website = "www.svetteh.ru" },
                new Manufacturer { Name = "VARTON", Country = "Россия", Website = "www.varton.ru" },
                new Manufacturer { Name = "Jazzway", Country = "Китай", Website = "www.jazzway.ru" },
            };
            db.Manufacturers.AddRange(manufacturers);
            db.SaveChanges();

            // характеристики 
            var chars = new List<Characteristic>
            {
                new Characteristic { Name = "Мощность", Unit = "Вт" },
                new Characteristic { Name = "Цветовая температура", Unit = "К" },
                new Characteristic { Name = "Световой поток", Unit = "лм" },
                new Characteristic { Name = "Напряжение", Unit = "В" },
                new Characteristic { Name = "Тип цоколя", Unit = "" },
                new Characteristic { Name = "Степень защиты IP", Unit = "" },
                new Characteristic { Name = "Материал", Unit = "" },
                new Characteristic { Name = "Цвет корпуса", Unit = "" },
                new Characteristic { Name = "Размер", Unit = "мм" },
                new Characteristic { Name = "Индекс цветопередачи CRI", Unit = "" },
            };
            db.Characteristics.AddRange(chars);
            db.SaveChanges();

            //стандартные значения характеристик
            var charValues = new List<CharacteristicValue>
            {
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "5" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "8" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "10" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "12" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "15" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "18" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "20" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "24" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "30" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "36" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "40" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "60" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "100" },
                new CharacteristicValue { CharacteristicId = chars[0].Id, Value = "150" },
                new CharacteristicValue { CharacteristicId = chars[1].Id, Value = "2700" },
                new CharacteristicValue { CharacteristicId = chars[1].Id, Value = "3000" },
                new CharacteristicValue { CharacteristicId = chars[1].Id, Value = "4000" },
                new CharacteristicValue { CharacteristicId = chars[1].Id, Value = "5000" },
                new CharacteristicValue { CharacteristicId = chars[1].Id, Value = "6500" },
                new CharacteristicValue { CharacteristicId = chars[4].Id, Value = "E27" },
                new CharacteristicValue { CharacteristicId = chars[4].Id, Value = "E14" },
                new CharacteristicValue { CharacteristicId = chars[4].Id, Value = "GU10" },
                new CharacteristicValue { CharacteristicId = chars[4].Id, Value = "GU5.3" },
                new CharacteristicValue { CharacteristicId = chars[4].Id, Value = "G13" },
                new CharacteristicValue { CharacteristicId = chars[4].Id, Value = "G9" },
                new CharacteristicValue { CharacteristicId = chars[5].Id, Value = "IP20" },
                new CharacteristicValue { CharacteristicId = chars[5].Id, Value = "IP40" },
                new CharacteristicValue { CharacteristicId = chars[5].Id, Value = "IP44" },
                new CharacteristicValue { CharacteristicId = chars[5].Id, Value = "IP54" },
                new CharacteristicValue { CharacteristicId = chars[5].Id, Value = "IP65" },
                new CharacteristicValue { CharacteristicId = chars[6].Id, Value = "Металл" },
                new CharacteristicValue { CharacteristicId = chars[6].Id, Value = "Алюминий" },
                new CharacteristicValue { CharacteristicId = chars[6].Id, Value = "Пластик" },
                new CharacteristicValue { CharacteristicId = chars[6].Id, Value = "Стекло" },
                new CharacteristicValue { CharacteristicId = chars[6].Id, Value = "Сталь" },
                new CharacteristicValue { CharacteristicId = chars[7].Id, Value = "Белый" },
                new CharacteristicValue { CharacteristicId = chars[7].Id, Value = "Черный" },
                new CharacteristicValue { CharacteristicId = chars[7].Id, Value = "Серый" },
                new CharacteristicValue { CharacteristicId = chars[7].Id, Value = "Хром" },
            };
            db.CharacteristicValues.AddRange(charValues);
            db.SaveChanges();

            //Импорт из JSON
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string jsonPath = Path.Combine(baseDir, "..", "..", "..", "Data", "interior_lighting.json");
            if (!File.Exists(jsonPath))
                jsonPath = Path.Combine(baseDir, "Data", "interior_lighting.json");
            string extraPath = Path.Combine(baseDir, "..", "..", "..", "Data", "extra_products.json");
            if (!File.Exists(extraPath))
                extraPath = Path.Combine(baseDir, "Data", "extra_products.json");
            string extraPath2 = Path.Combine(baseDir, "..", "..", "..", "Data", "extra_products2.json");
            if (!File.Exists(extraPath2))
                extraPath2 = Path.Combine(baseDir, "Data", "extra_products2.json");

            var items = new List<JsonItem>();

            if (File.Exists(jsonPath))
            {
                var jsonText = File.ReadAllText(jsonPath);
                var parsed = JsonSerializer.Deserialize<List<JsonItem>>(jsonText);
                if (parsed != null) items.AddRange(parsed);
            }

            if (File.Exists(extraPath))
            {
                var jsonText = File.ReadAllText(extraPath);
                var parsed = JsonSerializer.Deserialize<List<JsonItem>>(jsonText);
                if (parsed != null) items.AddRange(parsed);
            }

            if (File.Exists(extraPath2))
            {
                var jsonText = File.ReadAllText(extraPath2);
                var parsed = JsonSerializer.Deserialize<List<JsonItem>>(jsonText);
                if (parsed != null) items.AddRange(parsed);
            }

            if (items.Count == 0)
            {
                SeedMinimal(db, categories, manufacturers, chars, charValues, random);
                return;
            }

            int imported = 0;
            int maxImport = 400;

            foreach (var item in items)
            {
                if (imported >= maxImport) break;
                if (string.IsNullOrWhiteSpace(item.name)) continue;

                var props = item.properties?.Where(p =>
                    p.property != "КлючевыеСлова (Общие)" &&
                    p.property != "КлючевыеСлова" &&
                    p.property != "Наименование" &&
                    p.property != "Вид номенклатуры" &&
                    p.property != "Маркировка" &&
                    p.property != "Вид" &&
                    p.property != "Характеристика" &&
                    !string.IsNullOrWhiteSpace(p.value))
                    .ToList() ?? new List<JsonProperty>();

                if (props.Count == 0 && (item.price <= 0 || string.IsNullOrWhiteSpace(item.article)))
                    continue;

                int catIdx = GuessCategoryIndex(item.name ?? "", props);

                int manIdx = GuessManufacturerIndex(item.name ?? "", props, manufacturers);

                string article = string.IsNullOrWhiteSpace(item.article) ? $"IMP-{imported:D4}" : item.article!.Trim();
                decimal price = item.price > 0 ? item.price : (decimal)random.Next(200, 5000);
                int stock = (int)(item.remaining > 0 ? item.remaining : random.Next(5, 100));

                var product = new Product
                {
                    Article = article,
                    Name = item.name?.Trim() ?? "Без названия",
                    CategoryId = categories[catIdx].Id,
                    ManufacturerId = manufacturers[manIdx].Id,
                    Price = price,
                    StockQuantity = stock,
                    Description = $"Категория: {item.catalog ?? "Внутреннее освещение"}",
                    CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365))
                };
                db.Products.Add(product);
                db.SaveChanges();

                foreach (var prop in props)
                {
                    string propName = prop.property ?? "";
                    string propValue = prop.value?.Trim() ?? "";

                    var (chIdx, cleanValue) = MapProperty(propName, propValue, chars, charValues);
                    if (chIdx >= 0 && !string.IsNullOrWhiteSpace(cleanValue))
                    {
                        var cv = db.CharacteristicValues
                            .FirstOrDefault(cv => cv.CharacteristicId == chars[chIdx].Id && cv.Value == cleanValue);
                        if (cv != null)
                        {
                            if (!db.ProductCharacteristics.Any(pc => pc.ProductId == product.Id && pc.CharacteristicId == chars[chIdx].Id))
                            {
                                db.ProductCharacteristics.Add(new ProductCharacteristic
                                {
                                    ProductId = product.Id,
                                    CharacteristicId = chars[chIdx].Id,
                                    CharacteristicValueId = cv.Id
                                });
                            }
                        }
                    }
                }
                db.SaveChanges();
                imported++;
            }

            // пользователь
            if (!db.Users.Any(u => u.Login == "user"))
            {
                db.Users.Add(new User
                {
                    Login = "user",
                    PasswordHash = "user123",
                    Email = "user@mail.com",
                    FullName = "Иванов Иван Иванович",
                    Role = "user",
                    IsBlocked = false,
                    CreatedAt = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        private static int GuessCategoryIndex(string name, List<JsonProperty> props)
        {
            string lower = name.ToLower();
            if (lower.Contains("плафон") || lower.Contains("абажур")) return 0;
            if (lower.Contains("офисн") || lower.Contains("администрат") || lower.Contains("armstrong") || lower.Contains("панел 60")) return 1;
            if (lower.Contains("люстр")) return 2;
            if (lower.Contains("подвесн")) return 3;
            if (lower.Contains("потолоч") || lower.Contains("панел") || lower.Contains("ultra")) return 4;
            if (lower.Contains("настен") || lower.Contains("бра ") || lower.Contains("sconce") || lower.Contains("настенный")) return 5;
            if (lower.Contains("настольн") || lower.Contains("стол") || lower.Contains("на прищепк") || lower.Contains("на основании")) return 6;
            if (lower.Contains("торшер")) return 7;
            if (lower.Contains("спот") || lower.Contains("прожект") || lower.Contains("софит")) return 8;
            if (lower.Contains("крепл") || lower.Contains("кронштейн") || lower.Contains("держател") || lower.Contains("трансформатор") || lower.Contains("блок питан") || lower.Contains("шинопров") || lower.Contains("соединитель")) return 9;
            if (lower.Contains("встраив") || lower.Contains("точечн") || lower.Contains("downlight")) return 10;
            if (lower.Contains("трек") || lower.Contains("track")) return 11;
            // fallback по ключевым словам из названия
            if (lower.Contains("декорат") || lower.Contains("galaxy") || lower.Contains("love ") || lower.Contains("шарик") || lower.Contains("бабочк") || lower.Contains("футбольн") || lower.Contains("прикроватн")) return 7;
            if (lower.Contains("лампа ") || lower.Contains("светодиодн") || lower.Contains("led")) return 4;
            return 2; // люстры по умолчанию
        }

        private static int GuessManufacturerIndex(string name, List<JsonProperty> props, List<Manufacturer> manufacturers)
        {
            string lower = name.ToLower();
            if (lower.Contains("iek")) return 0;
            if (lower.Contains("volpe")) return 1;
            if (lower.Contains("tdm") || lower.Contains("тдм")) return 2;
            if (lower.Contains("uniel")) return 3;
            if (lower.Contains("feron")) return 4;
            if (lower.Contains("gauss")) return 5;
            if (lower.Contains("general")) return 6;
            if (lower.Contains("световые технологи") || lower.Contains("светотех")) return 7;
            if (lower.Contains("varton") || lower.Contains("вартон")) return 8;
            if (lower.Contains("jazzway")) return 9;
            if (lower.Contains("apeyron")) return 0;
            if (lower.Contains("ambrel")) return 1;
            if (lower.Contains("maytoni")) return 7;
            if (lower.Contains("globo")) return 8;
            if (lower.Contains("eglo")) return 9;
            if (lower.Contains("philip")) return 5;
            if (lower.Contains("osram")) return 5;
            return random.Next(manufacturers.Count);
        }

        private static Random random = new Random();

        private static (int charIdx, string cleanValue) MapProperty(string propName, string propValue, List<Characteristic> chars, List<CharacteristicValue> charValues)
        {
            string val = propValue.Trim();
            if (string.IsNullOrWhiteSpace(val)) return (-1, "");

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"Мощность", "Мощность"},
                {"Мощность, Вт", "Мощность"},
                {"Мощность Вт", "Мощность"},
                {"Температура", "Цветовая температура"},
                {"Цветовая температура", "Цветовая температура"},
                {"Световой поток", "Световой поток"},
                {"Напряжение", "Напряжение"},
                {"Цоколь", "Тип цоколя"},
                {"Тип цоколя", "Тип цоколя"},
                {"Степень защиты", "Степень защиты IP"},
                {"IP", "Степень защиты IP"},
                {"Материал", "Материал"},
                {"Материал корпуса", "Материал"},
                {"Цвет корпуса", "Цвет корпуса"},
                {"Цвет", "Цвет корпуса"},
                {"Размер", "Размер"},
                {"CRI", "Индекс цветопередачи CRI"},
            };

            if (!map.TryGetValue(propName, out string? mappedName))
                return (-1, "");

            var ch = chars.FirstOrDefault(c => c.Name == mappedName);
            if (ch == null) return (-1, "");

            string clean = val;
            clean = Regex.Replace(clean, @"\s*мм$", "", RegexOptions.IgnoreCase);
            clean = Regex.Replace(clean, @"\s*[Вв][Тт]$", "", RegexOptions.IgnoreCase);
            clean = Regex.Replace(clean, @"\s*[Кк]$", "", RegexOptions.IgnoreCase);
            clean = Regex.Replace(clean, @"\s*[Лл][Мм]$", "", RegexOptions.IgnoreCase);
            clean = Regex.Replace(clean, @"\s*[Вв]$", "", RegexOptions.IgnoreCase);
            clean = clean.Trim();

            var existing = charValues.FirstOrDefault(cv => cv.CharacteristicId == ch.Id && cv.Value == clean);
            if (existing != null)
                return (chars.IndexOf(ch), clean);

            return (chars.IndexOf(ch), clean);
        }

        private static void SeedMinimal(AppDbContext db, List<Category> categories, List<Manufacturer> manufacturers,
            List<Characteristic> chars, List<CharacteristicValue> charValues, Random random)
        {
            for (int i = 0; i < 20; i++)
            {
                db.Products.Add(new Product
                {
                    Article = $"SMP-{i:D4}",
                    Name = $"Светильник LED тестовый {i + 1}",
                    CategoryId = categories[i % categories.Count].Id,
                    ManufacturerId = manufacturers[i % manufacturers.Count].Id,
                    Price = random.Next(200, 8000),
                    StockQuantity = random.Next(5, 100),
                    CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365))
                });
            }
            db.SaveChanges();
        }
    }
}
