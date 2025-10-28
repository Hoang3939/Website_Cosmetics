# Website Cosmetics Database

This directory contains the database scripts for the Website Cosmetics application.

## 📁 Files Overview

| File | Description | Purpose |
|------|-------------|---------|
| `01_CreateTables.sql` | Database schema creation | Creates all tables, indexes, and constraints |
| `02_InsertData.sql` | Sample data insertion | Inserts sample data and creates default accounts |
| `README.md` | This documentation | Usage instructions and database information |

## 🚀 Quick Start

### Prerequisites
- SQL Server 2019 or later
- SQL Server Management Studio (SSMS) or sqlcmd
- Database server access credentials

### Installation Steps

1. **Create Database Schema**
   ```sql
   -- Run this first
   sqlcmd -S [SERVER_NAME] -U [USERNAME] -P [PASSWORD] -i "01_CreateTables.sql"
   ```

2. **Insert Sample Data**
   ```sql
   -- Run this second
   sqlcmd -S [SERVER_NAME] -U [USERNAME] -P [PASSWORD] -i "02_InsertData.sql"
   ```

3. **Verify Installation**
   ```sql
   USE WebsiteCosmetic;
   SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
   ```

## 📊 Database Schema

### Authentication & Authorization Tables
- **Users** - User accounts and profiles
- **Roles** - User roles (Admin, Staff, User)
- **UserRoles** - User-role assignments
- **PasswordResetTokens** - Password reset functionality
- **EmailConfirmationTokens** - Email confirmation system

### Permission System Tables
- **Permissions** - System permissions
- **RolePermissions** - Role-permission assignments
- **UserPermissions** - Individual user permissions

### Product & Business Tables
- **Brand** - Product brands
- **Category** - Product categories
- **Product** - Product information
- **ProductImage** - Product images

## 🔐 Default Accounts

After running the data insertion script, the following accounts are created:

### Admin Account
- **Username:** `admin`
- **Email:** `admin@cosmetics.com`
- **Password:** `Admin123!`
- **Role:** Admin
- **Access:** Full system access

### Staff Account
- **Username:** `staff`
- **Email:** `staff@cosmetics.com`
- **Password:** `Staff123!`
- **Role:** Staff
- **Access:** Limited admin access

## 📝 Sample Data

The database includes sample data for:

### Brands (5)
- L'Oréal (France)
- Maybelline (USA)
- MAC Cosmetics (Canada)
- NARS (France)
- Fenty Beauty (USA)

### Categories (5)
- Lipstick
- Eyeshadow
- Blush
- Foundation
- Setting Powder

### Products (5)
- Maybelline SuperStay Matte Ink ($15.00)
- L'Oréal Paradise Enchanted Palette ($18.00)
- NARS Orgasm Blush ($32.00)
- MAC Studio Fix Fluid SPF15 ($35.00)
- Fenty Beauty Pro Filt'r Powder ($36.00)

## 🛠️ Database Management

### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=[SERVER];Database=WebsiteCosmetic;User Id=[USER];Password=[PASSWORD];TrustServerCertificate=true;"
  }
}
```

### Backup Database
```sql
BACKUP DATABASE WebsiteCosmetic 
TO DISK = 'C:\Backup\WebsiteCosmetic.bak'
WITH FORMAT, INIT, NAME = 'WebsiteCosmetic Full Backup';
```

### Restore Database
```sql
RESTORE DATABASE WebsiteCosmetic 
FROM DISK = 'C:\Backup\WebsiteCosmetic.bak'
WITH REPLACE;
```

## 🔧 Troubleshooting

### Common Issues

1. **"Invalid object name" Error**
   - Ensure you've run `01_CreateTables.sql` first
   - Check if the database exists and is selected

2. **Login Failed**
   - Verify the default accounts are created
   - Check password hashing (uses BCrypt)

3. **Permission Denied**
   - Ensure user has appropriate database permissions
   - Check if the user account is active

### Reset Database
```sql
-- Drop and recreate database (WARNING: This will delete all data)
DROP DATABASE IF EXISTS WebsiteCosmetic;
-- Then run 01_CreateTables.sql and 02_InsertData.sql again
```

## 📈 Performance Optimization

The database includes the following indexes for optimal performance:

- **User lookups:** Email, Username
- **Token searches:** Password reset, Email confirmation
- **Product searches:** Name, Slug, Brand, Category
- **Permission checks:** Role and user permissions

## 🔄 Migration Notes

### From Previous Versions
If migrating from an older version:

1. Backup existing data
2. Run new table creation script
3. Migrate data using appropriate transformation scripts
4. Verify data integrity

### Entity Framework Migrations
The application uses Entity Framework Core migrations. To update the database schema:

```bash
dotnet ef database update -c ApplicationDbContext
```

## 📞 Support

For database-related issues:
1. Check this README first
2. Review the SQL scripts for syntax errors
3. Verify connection string configuration
4. Check SQL Server logs for detailed error messages

## 📋 Version History

- **v1.0** - Initial database schema with authentication and product management
- **v1.1** - Added permission system and role-based access control
- **v1.2** - Updated to use GUID primary keys and improved indexing

---

**Note:** Always backup your database before making structural changes!