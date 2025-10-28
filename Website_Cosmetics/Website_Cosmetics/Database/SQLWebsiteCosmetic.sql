CREATE DATABASE WebsiteCosmetic;
GO
USE WebsiteCosmetic;
GO

CREATE TABLE Brand (
    BrandId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Country NVARCHAR(100),
    IsActive BIT DEFAULT 1
);

CREATE TABLE Product (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Slug NVARCHAR(200) NULL,
    Price DECIMAL(12,2) NOT NULL DEFAULT 0,
    Description NVARCHAR(MAX) NULL,
    Ingredients NVARCHAR(MAX) NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    BrandId INT,
    CONSTRAINT FK_Product_Brand FOREIGN KEY (BrandId)
        REFERENCES Brand(BrandId)
        ON DELETE SET NULL
);

CREATE TABLE ProductImage (
    ProductImageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    Url NVARCHAR(255) NOT NULL,
    IsCover BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_ProductImage_Product FOREIGN KEY (ProductId)
        REFERENCES Product(ProductId)
        ON DELETE CASCADE
);

CREATE TABLE Category (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    IsActive BIT DEFAULT 1
);

ALTER TABLE Product
ADD CategoryId INT NULL;

ALTER TABLE Product
ADD CONSTRAINT FK_Product_Category 
FOREIGN KEY (CategoryId) 
REFERENCES Category(CategoryId)
ON DELETE SET NULL;

-- Thêm dữ liệu mẫu cho bảng Brand
INSERT INTO Brand (Name, Country) VALUES
('L’Oréal', 'France'),
('Maybelline', 'USA'),
('MAC Cosmetics', 'Canada'),
('NARS', 'France'),
('Fenty Beauty', 'USA');

-- Thêm dữ liệu mẫu cho bảng Category
INSERT INTO Category (Name, Description) VALUES
('Son môi', 'Các loại son lì, son bóng, son tint'),
('Phấn mắt', 'Bảng màu mắt đa dạng tone'),
('Má hồng', 'Phấn má dạng nén và dạng kem'),
('Kem nền', 'Kem nền cho mọi loại da'),
('Phấn phủ', 'Phấn phủ kiềm dầu, mịn da');

-- Thêm dữ liệu mẫu cho bảng Product
INSERT INTO Product (Name, Slug, Price, Description, Ingredients, BrandId, CategoryId)
VALUES
('Son lì Maybelline SuperStay Matte Ink', 'son-li-maybelline-superstay', 259000,
 'Son lì lâu trôi, màu chuẩn, bền đẹp suốt 16 giờ.',
 'Isododecane, Dimethicone, Silica.', 2, 1),

('Bảng phấn mắt L’Oréal Paradise Enchanted', 'phan-mat-loreal-paradise', 399000,
 'Bảng phấn mắt 12 màu tông ấm, dễ tán và lâu trôi.', 
 'Talc, Mica, Dimethicone.', 1, 2),

('Phấn má hồng NARS Orgasm', 'ma-hong-nars-orgasm', 850000,
 'Phấn má hồng biểu tượng của NARS với ánh nhũ nhẹ.', 
 'Talc, Nylon-12, Dimethicone.', 4, 3),

('Kem nền MAC Studio Fix Fluid SPF15', 'kem-nen-mac-studio-fix', 950000,
 'Kem nền che phủ tốt, kiềm dầu, mịn da.', 
 'Water, Cyclopentasiloxane, Talc.', 3, 4),

('Phấn phủ Fenty Beauty Pro Filt’r', 'phan-phu-fenty-beauty', 890000,
 'Phấn phủ mịn nhẹ, giữ lớp nền lâu trôi.', 
 'Silica, Dimethicone, Mica.', 5, 5);

-- Thêm hình ảnh minh họa cho mỗi sản phẩm
INSERT INTO ProductImage (ProductId, Url, IsCover)
VALUES
(1, 'https://example.com/images/son-maybelline.jpg', 1),
(2, 'https://example.com/images/phan-mat-loreal.jpg', 1),
(3, 'https://example.com/images/ma-hong-nars.jpg', 1),
(4, 'https://example.com/images/kem-nen-mac.jpg', 1),
(5, 'https://example.com/images/phan-phu-fenty.jpg', 1);
