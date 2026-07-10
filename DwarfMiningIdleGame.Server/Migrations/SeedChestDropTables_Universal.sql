-- SQL Скрипт для заполнения ChestDropTables и DropEntry
-- Универсальный для SQLite, SQL Server и PostgreSQL
-- GUIDы генерируются автоматически

-- ============================================
-- Уровень 1 - Обычные сундуки
-- ============================================

-- Вставить ChestDropTable для уровня 1
INSERT INTO ChestDropTables (Id, ChestLevel)
VALUES (NEWID(), 1);

-- Вставить DropEntry для уровня 1
-- Запись 1: Equipment с вероятностью 70%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, Rarity, ItemLevel)
SELECT NEWID(), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 1 ORDER BY ChestLevel DESC LIMIT 1),
       0.7, 1, 2, 0, 0, 1;

-- Запись 2: Material с вероятностью 50%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, Rarity, ItemLevel)
SELECT NEWID(), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 1 ORDER BY ChestLevel DESC LIMIT 1),
       0.5, 2, 4, 1, 1, 1;

-- ============================================
-- Уровень 2 - Редкие сундуки
-- ============================================

-- Вставить ChestDropTable для уровня 2
INSERT INTO ChestDropTables (Id, ChestLevel)
VALUES (NEWID(), 2);

-- Вставить DropEntry для уровня 2
-- Запись 1: Equipment Uncommon с вероятностью 60%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, Rarity, ItemLevel)
SELECT NEWID(), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 2 ORDER BY ChestLevel DESC LIMIT 1),
       0.6, 1, 2, 0, 1, 5;

-- Запись 2: Material Uncommon с вероятностью 45%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, Rarity, ItemLevel)
SELECT NEWID(), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 2 ORDER BY ChestLevel DESC LIMIT 1),
       0.45, 1, 3, 1, 1, 5;

-- Запись 3: Equipment Rare с вероятностью 25%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, Rarity, ItemLevel)
SELECT NEWID(), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 2 ORDER BY ChestLevel DESC LIMIT 1),
       0.25, 1, 1, 0, 2, 7;

-- ============================================
-- Уровень 3 - Легендарные сундуки
-- ============================================

-- Вставить ChestDropTable для уровня 3
INSERT INTO ChestDropTables (Id, ChestLevel)
VALUES (NEWID(), 3);

-- Вставить DropEntry для уровня 3
-- Запись 1: Equipment Rare с вероятностью 55%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, Rarity, ItemLevel)
SELECT NEWID(), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 3 ORDER BY ChestLevel DESC LIMIT 1),
       0.55, 1, 2, 0, 2, 10;

-- Запись 2: Material Rare с вероятностью 40%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, Rarity, ItemLevel)
SELECT NEWID(), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 3 ORDER BY ChestLevel DESC LIMIT 1),
       0.4, 1, 2, 1, 2, 10;

-- Запись 3: Equipment Epic с вероятностью 20%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, Rarity, ItemLevel)
SELECT NEWID(), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 3 ORDER BY ChestLevel DESC LIMIT 1),
       0.2, 1, 1, 0, 3, 12;

-- Запись 4: Equipment Legendary с вероятностью 5%
INSERT INTO DropEntry (Id, ChestDropTableId, Probability, MinAmount, MaxAmount, ItemType, Rarity, ItemLevel)
SELECT NEWID(), 
       (SELECT Id FROM ChestDropTables WHERE ChestLevel = 3 ORDER BY ChestLevel DESC LIMIT 1),
       0.05, 1, 1, 0, 4, 15;

-- ============================================
-- Проверка данных (выполнить после вставки)
-- ============================================

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
