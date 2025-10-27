# 🔒 Data Validation Rules

## 📋 Validation Rules Summary

### 👤 **User Registration & Login**

#### **Username**
- **Required**: Yes
- **Length**: 3-50 characters
- **Pattern**: Letters, numbers, and underscores only
- **Regex**: `^[a-zA-Z0-9_]+$`
- **Examples**: ✅ `john_doe`, `user123`, `admin` | ❌ `john-doe`, `user@123`, `ab`

#### **Email**
- **Required**: Yes
- **Length**: Max 100 characters
- **Format**: Valid email format
- **Examples**: ✅ `user@example.com` | ❌ `invalid-email`, `user@`

#### **Password**
- **Required**: Yes
- **Length**: 6-100 characters
- **Requirements**:
  - At least 1 uppercase letter (A-Z)
  - At least 1 lowercase letter (a-z)
  - At least 1 number (0-9)
  - At least 1 special character (@$!%*?&)
- **Regex**: `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]`
- **Examples**: ✅ `Password123!` | ❌ `password`, `PASSWORD123`, `Password123`

#### **First Name & Last Name**
- **Required**: No
- **Length**: 2-50 characters
- **Pattern**: Letters and spaces only
- **Regex**: `^[a-zA-Z\s]+$`
- **Examples**: ✅ `John`, `Mary Jane` | ❌ `John123`, `Mary-Jane`, `J`

#### **Phone Number**
- **Required**: No
- **Length**: 10-11 digits
- **Pattern**: Numbers only
- **Regex**: `^[0-9]+$`
- **Examples**: ✅ `0123456789`, `0987654321` | ❌ `012-345-6789`, `+84123456789`, `123`

### 🏷️ **Roles & Permissions**

#### **Role Name**
- **Required**: Yes
- **Length**: 2-50 characters
- **Pattern**: Letters and spaces only
- **Regex**: `^[a-zA-Z\s]+$`
- **Examples**: ✅ `Admin`, `Staff Member` | ❌ `Admin123`, `Staff-Member`

#### **Permission Name**
- **Required**: Yes
- **Length**: 3-100 characters
- **Pattern**: Letters, numbers, dots, and underscores
- **Regex**: `^[a-zA-Z0-9._]+$`
- **Examples**: ✅ `Admin.Dashboard.View`, `User.Manage` | ❌ `Admin-Dashboard`, `User@Manage`

#### **Category**
- **Required**: No
- **Length**: Max 50 characters
- **Pattern**: Letters and spaces only
- **Regex**: `^[a-zA-Z\s]+$`
- **Examples**: ✅ `Admin`, `User Management` | ❌ `Admin123`, `User-Management`

## 🚨 **Common Validation Errors**

### **Password Validation Failures**
```
❌ "password" - Missing uppercase, number, special character
❌ "PASSWORD123" - Missing lowercase, special character  
❌ "Password123" - Missing special character
❌ "Pass1!" - Too short (less than 6 characters)
```

### **Phone Number Validation Failures**
```
❌ "012-345-6789" - Contains hyphens
❌ "+84123456789" - Contains plus sign
❌ "123" - Too short
❌ "012345678901" - Too long
```

### **Name Validation Failures**
```
❌ "John123" - Contains numbers
❌ "Mary-Jane" - Contains hyphen
❌ "J" - Too short
❌ "A" - Too short
```

## 🔧 **Implementation Notes**

### **Client-Side Validation**
- All validation rules are enforced on both client and server
- Real-time validation feedback in forms
- Clear error messages for each field

### **Server-Side Validation**
- Additional security layer
- Prevents malicious data injection
- Consistent validation across all endpoints

### **Database Constraints**
- String length limits enforced at database level
- Regular expression patterns validated in application layer
- Foreign key constraints for data integrity

## 📝 **Testing Examples**

### **Valid Test Data**
```json
{
  "username": "testuser123",
  "email": "test@example.com",
  "password": "TestPass123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "0123456789"
}
```

### **Invalid Test Data**
```json
{
  "username": "ab",  // Too short
  "email": "invalid",  // Invalid format
  "password": "weak",  // Missing requirements
  "firstName": "John123",  // Contains numbers
  "phoneNumber": "012-345-6789"  // Contains hyphens
}
```

## 🛡️ **Security Benefits**

1. **Prevents SQL Injection**: Input sanitization
2. **Prevents XSS**: Character restrictions
3. **Data Integrity**: Consistent format validation
4. **User Experience**: Clear error messages
5. **System Stability**: Prevents invalid data processing
