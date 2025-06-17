-- Test data for Jewelry Store
-- Run this after the database is created and migrations applied

USE JewelryStoreDB;

-- Insert test products
INSERT INTO Products (Name, Description, Price, Material, Gemstone, Category, Weight, Size, StockQuantity, ImageUrl, IsActive, CreatedAt)
VALUES 
-- Rings
('Золотое кольцо с бриллиантом', 'Изысканное кольцо из белого золота 18К с бриллиантом 0.5 карата', 95000.00, 'Белое золото 18К', 'Бриллиант', 'Кольца', 3.2, '17', 5, '/images/diamond-ring.jpg', 1, GETUTCDATE()),
('Серебряное кольцо с изумрудом', 'Элегантное кольцо из серебра 925 пробы с натуральным изумрудом', 25000.00, 'Серебро 925', 'Изумруд', 'Кольца', 2.8, '16', 8, '/images/emerald-ring.jpg', 1, GETUTCDATE()),
('Платиновое обручальное кольцо', 'Классическое обручальное кольцо из платины с гравировкой', 75000.00, 'Платина 950', '', 'Кольца', 4.1, '18', 3, '/images/wedding-ring.jpg', 1, GETUTCDATE()),

-- Earrings
('Серьги с сапфирами', 'Изящные серьги-гвоздики с синими сапфирами в золотой оправе', 45000.00, 'Желтое золото 14К', 'Сапфир', 'Серьги', 1.5, 'One Size', 12, '/images/sapphire-earrings.jpg', 1, GETUTCDATE()),
('Серебряные серьги-капли', 'Стильные серьги-капли из серебра с фианитами', 8500.00, 'Серебро 925', 'Фианит', 'Серьги', 2.1, 'One Size', 15, '/images/silver-drop-earrings.jpg', 1, GETUTCDATE()),

-- Necklaces
('Золотая цепочка Венецианское плетение', 'Классическая золотая цепочка венецианского плетения', 65000.00, 'Желтое золото 18К', '', 'Цепочки', 12.5, '50 см', 7, '/images/gold-chain.jpg', 1, GETUTCDATE()),
('Колье с жемчугом', 'Роскошное колье из натурального морского жемчуга', 120000.00, 'Белое золото 18К', 'Жемчуг', 'Колье', 25.3, '45 см', 2, '/images/pearl-necklace.jpg', 1, GETUTCDATE()),

-- Bracelets
('Браслет Пандора', 'Серебряный браслет с подвесками-шармами', 15000.00, 'Серебро 925', '', 'Браслеты', 18.7, '19 см', 10, '/images/pandora-bracelet.jpg', 1, GETUTCDATE()),
('Золотой браслет с рубинами', 'Роскошный золотой браслет с натуральными рубинами', 180000.00, 'Желтое золото 18К', 'Рубин', 'Браслеты', 28.4, '18 см', 1, '/images/ruby-bracelet.jpg', 1, GETUTCDATE()),

-- Watches
('Женские золотые часы', 'Элегантные женские часы из золота со швейцарским механизмом', 250000.00, 'Желтое золото 18К', '', 'Часы', 45.2, 'One Size', 3, '/images/gold-watch.jpg', 1, GETUTCDATE());

-- Insert a test user (password is "Password123!" hashed with BCrypt)
-- Note: In real application, users should register through the API
INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Phone, Address, CreatedAt, IsActive)
VALUES 
('testuser', 'test@example.com', '$2a$11$vT8wrm9j3zXzKm9uD.xXle5Bv4Rt7OOgn9Gs1lKr2O0cQGNgI5K7m', 'Тест', 'Пользователь', '+7-900-123-45-67', 'г. Москва, ул. Тестовая, д. 1', GETUTCDATE(), 1),
('admin', 'admin@jewelrystore.com', '$2a$11$vT8wrm9j3zXzKm9uD.xXle5Bv4Rt7OOgn9Gs1lKr2O0cQGNgI5K7m', 'Админ', 'Администратор', '+7-900-000-00-00', 'г. Москва, ул. Главная, д. 1', GETUTCDATE(), 1);

PRINT 'Test data inserted successfully!';
PRINT 'Test user credentials:';
PRINT '  Username: testuser';
PRINT '  Password: Password123!';
PRINT '';
PRINT 'Admin credentials:';
PRINT '  Username: admin';  
PRINT '  Password: Password123!';

SELECT 'Products inserted: ' + CAST(COUNT(*) AS VARCHAR) AS Result FROM Products;
SELECT 'Users inserted: ' + CAST(COUNT(*) AS VARCHAR) AS Result FROM Users; 