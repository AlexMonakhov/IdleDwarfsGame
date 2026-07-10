-- SQL Скрипт для заполнения ChestDropTables и DropEntry
-- Для SQLite базы данных
-- Выполнить этот скрипт после применения миграции 20260416194440_AddDropEntry

-- ============================================
-- Уровень 1 - Обычные сундуки
-- ============================================

-- Вставить ChestDropTable для уровня 1
INSERT INTO ChestDropTables (Id, ChestLevel, ChestType)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 1, 0;

-- Получить Id последней вставленной записи для уровня 1
-- (Сохраняем его в переменную для использования в DropEntry)
-- Для SQLite используем способ через временную таблицу

-- Вставить DropEntry для уровня 1
-- Запись 1: Equipment Armor с вероятностью 70%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, SubType, Rarity, ItemLevel)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 1 LIMIT 1),
       0.7, 1, 2, 0, 'Armor', 0, 1;

-- Запись 2: Material Ore с вероятностью 50%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, SubType, Rarity, ItemLevel)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 1 LIMIT 1),
       0.5, 2, 4, 1, 'Ore', 0, 1;

-- ============================================
-- Уровень 2 - Редкие сундуки
-- ============================================

-- Вставить ChestDropTable для уровня 2
INSERT INTO ChestDropTables (Id, ChestLevel, ChestType)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 2, 1;

-- Вставить DropEntry для уровня 2
-- Запись 1: Equipment Weapon Uncommon с вероятностью 60%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, SubType, Rarity, ItemLevel)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 2 LIMIT 1),
       0.6, 1, 2, 0, 'Weapon', 1, 5;

-- Запись 2: Material Gem Uncommon с вероятностью 45%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, SubType, Rarity, ItemLevel)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 2 LIMIT 1),
       0.45, 1, 3, 1, 'Gem', 1, 5;

-- Запись 3: Equipment Accessory Rare с вероятностью 25%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, SubType, Rarity, ItemLevel)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 2 LIMIT 1),
       0.25, 1, 1, 0, 'Accessory', 2, 7;

-- ============================================
-- Уровень 3 - Легендарные сундуки
-- ============================================

-- Вставить ChestDropTable для уровня 3
INSERT INTO ChestDropTables (Id, ChestLevel, ChestType)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 3, 2;

-- Вставить DropEntry для уровня 3
-- Запись 1: Equipment Weapon Rare с вероятностью 55%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, SubType, Rarity, ItemLevel)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 3 LIMIT 1),
       0.55, 1, 2, 0, 'Weapon', 2, 10;

-- Запись 2: Material Herb Rare с вероятностью 40%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, SubType, Rarity, ItemLevel)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 3 LIMIT 1),
       0.4, 1, 2, 1, 'Herb', 2, 10;

-- Запись 3: Crystal Ruby Epic с вероятностью 20%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, SubType, Rarity, ItemLevel)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 3 LIMIT 1),
       0.2, 1, 1, 2, 'Ruby', 3, 12;

-- Запись 4: Crystal Diamond Legendary с вероятностью 5%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, SubType, Rarity, ItemLevel)
SELECT lower(
    substr(hex(randomblob(16)), 1, 8) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(4)), 1, 4) || '-' ||
    substr(hex(randomblob(12)), 1, 12)
), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 3 LIMIT 1),
       0.05, 1, 1, 2, 'Diamond', 4, 15;

-- ============================================
-- Проверка данных
-- ============================================

-- Для проверки выполните эти запросы:
-- SELECT * FROM ChestDropTables;
-- SELECT * FROM DropEntry;

-- SELECT 
--     cdt.Id, 
--     cdt.ChestLevel, 
--     COUNT(de.Id) as EntryCount
-- FROM ChestDropTables cdt
-- LEFT JOIN DropEntry de ON cdt.Id = de.ChestDropTableId
-- GROUP BY cdt.Id, cdt.ChestLevel
-- ORDER BY cdt.ChestLevel;
