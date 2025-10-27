# 🧪 Test Cases for Admin Pages

## 📋 Overview
This document outlines comprehensive test cases for all admin functionality in the Cosmetic Store website.

## 🎯 Test Environment Setup

### **Prerequisites**
- Admin user account with full permissions
- Staff user account with limited permissions
- Regular user account (for negative testing)
- Database with sample data

### **Test Data**
```json
{
  "admin": {
    "username": "admin",
    "password": "Admin123!",
    "email": "admin@cosmetics.com",
    "role": "Admin"
  },
  "staff": {
    "username": "staff",
    "password": "Staff123!",
    "email": "staff@cosmetics.com",
    "role": "Staff"
  },
  "user": {
    "username": "user",
    "password": "User123!",
    "email": "user@cosmetics.com",
    "role": "User"
  }
}
```

---

## 🔐 **Authentication & Authorization Tests**

### **TC-AUTH-001: Admin Login**
- **Objective**: Verify admin can log in successfully
- **Steps**:
  1. Navigate to `/Auth/Login`
  2. Enter admin credentials
  3. Click "Sign in"
- **Expected Result**: Redirected to `/Admin/Dashboard`
- **Priority**: High

### **TC-AUTH-002: Staff Login**
- **Objective**: Verify staff can log in successfully
- **Steps**:
  1. Navigate to `/Auth/Login`
  2. Enter staff credentials
  3. Click "Sign in"
- **Expected Result**: Redirected to `/Admin/StaffDashboard`
- **Priority**: High

### **TC-AUTH-003: Regular User Login**
- **Objective**: Verify regular user cannot access admin area
- **Steps**:
  1. Navigate to `/Auth/Login`
  2. Enter user credentials
  3. Click "Sign in"
- **Expected Result**: Redirected to `/Products` (not admin area)
- **Priority**: High

### **TC-AUTH-004: Unauthorized Access Prevention**
- **Objective**: Verify unauthorized users cannot access admin pages
- **Steps**:
  1. Navigate directly to `/Admin/Dashboard` without login
  2. Try to access `/Admin/StaffDashboard`
  3. Try to access `/Admin/Products`
- **Expected Result**: Redirected to login page
- **Priority**: Critical

---

## 📊 **Dashboard Tests**

### **TC-DASH-001: Admin Dashboard Access**
- **Objective**: Verify admin dashboard loads correctly
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Dashboard`
- **Expected Result**: 
  - Dashboard loads with KPI cards
  - Revenue, Orders, New Users, Low Stock metrics displayed
  - Recent Orders table visible
- **Priority**: High

### **TC-DASH-002: Staff Dashboard Access**
- **Objective**: Verify staff dashboard loads correctly
- **Steps**:
  1. Login as staff
  2. Navigate to `/Admin/StaffDashboard`
- **Expected Result**:
  - Dashboard loads with staff-specific KPIs
  - My Tasks, Orders Processed, Products Managed, Customer Inquiries displayed
  - Recent Activities table visible
- **Priority**: High

### **TC-DASH-003: Dashboard Data Accuracy**
- **Objective**: Verify dashboard displays correct data
- **Steps**:
  1. Login as admin
  2. Check KPI values against database
  3. Verify recent orders match actual orders
- **Expected Result**: All displayed data matches database records
- **Priority**: Medium

---

## 👥 **User Management Tests**

### **TC-USER-001: View Users List**
- **Objective**: Verify admin can view all users
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Users`
- **Expected Result**: 
  - Users table displays all registered users
  - Columns: Username, Email, Role, Status, Created Date
  - Pagination works correctly
- **Priority**: High

### **TC-USER-002: Edit User Role**
- **Objective**: Verify admin can change user roles
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Users`
  3. Click "Edit" on a user
  4. Change role from "User" to "Staff"
  5. Save changes
- **Expected Result**: User role updated successfully
- **Priority**: High

### **TC-USER-003: Deactivate User**
- **Objective**: Verify admin can deactivate users
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Users`
  3. Click "Deactivate" on a user
  4. Confirm action
- **Expected Result**: User status changed to inactive
- **Priority**: Medium

### **TC-USER-004: Staff Permission Restriction**
- **Objective**: Verify staff cannot manage users
- **Steps**:
  1. Login as staff
  2. Try to navigate to `/Admin/Users`
- **Expected Result**: Access denied or redirected
- **Priority**: High

---

## 🛍️ **Product Management Tests**

### **TC-PROD-001: View Products List**
- **Objective**: Verify admin can view all products
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Products`
- **Expected Result**: 
  - Products table displays all products
  - Columns: Name, Category, Price, Stock, Status
  - Search and filter functionality works
- **Priority**: High

### **TC-PROD-002: Add New Product**
- **Objective**: Verify admin can add new products
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Products`
  3. Click "Add Product"
  4. Fill in product details
  5. Save product
- **Expected Result**: New product added successfully
- **Priority**: High

### **TC-PROD-003: Edit Product**
- **Objective**: Verify admin can edit existing products
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Products`
  3. Click "Edit" on a product
  4. Modify product details
  5. Save changes
- **Expected Result**: Product updated successfully
- **Priority**: High

### **TC-PROD-004: Staff Product Management**
- **Objective**: Verify staff can manage products (if permitted)
- **Steps**:
  1. Login as staff
  2. Navigate to `/Admin/Products`
- **Expected Result**: 
  - If permitted: Can view and edit products
  - If not permitted: Access denied
- **Priority**: Medium

---

## 📦 **Order Management Tests**

### **TC-ORDER-001: View Orders List**
- **Objective**: Verify admin can view all orders
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Orders`
- **Expected Result**: 
  - Orders table displays all orders
  - Columns: Order ID, Customer, Total, Status, Date
  - Status filter works correctly
- **Priority**: High

### **TC-ORDER-002: Update Order Status**
- **Objective**: Verify admin can update order status
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Orders`
  3. Click "Edit" on an order
  4. Change status from "Processing" to "Shipped"
  5. Save changes
- **Expected Result**: Order status updated successfully
- **Priority**: High

### **TC-ORDER-003: Staff Order Processing**
- **Objective**: Verify staff can process orders
- **Steps**:
  1. Login as staff
  2. Navigate to `/Admin/Orders`
- **Expected Result**: 
  - Can view orders
  - Can update order status
  - Cannot delete orders
- **Priority**: Medium

---

## 🔒 **Permission Management Tests**

### **TC-PERM-001: View Permissions**
- **Objective**: Verify admin can view all permissions
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Roles`
- **Expected Result**: 
  - Permissions list displayed
  - Role assignments visible
  - Permission categories organized
- **Priority**: High

### **TC-PERM-002: Assign Role Permissions**
- **Objective**: Verify admin can assign permissions to roles
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Roles`
  3. Select a role
  4. Assign/remove permissions
  5. Save changes
- **Expected Result**: Role permissions updated successfully
- **Priority**: High

### **TC-PERM-003: Staff Permission Inheritance**
- **Objective**: Verify staff inherits role permissions
- **Steps**:
  1. Login as admin
  2. Assign permissions to Staff role
  3. Login as staff
  4. Test assigned permissions
- **Expected Result**: Staff can access permitted features only
- **Priority**: High

---

## 📈 **Reports Tests**

### **TC-REPORT-001: View Sales Reports**
- **Objective**: Verify admin can view sales reports
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Reports`
- **Expected Result**: 
  - Sales reports displayed
  - Date range filters work
  - Export functionality available
- **Priority**: Medium

### **TC-REPORT-002: Generate Custom Reports**
- **Objective**: Verify admin can generate custom reports
- **Steps**:
  1. Login as admin
  2. Navigate to `/Admin/Reports`
  3. Select custom date range
  4. Choose report type
  5. Generate report
- **Expected Result**: Custom report generated successfully
- **Priority**: Low

---

## 🚨 **Error Handling Tests**

### **TC-ERROR-001: Invalid Permission Access**
- **Objective**: Verify proper error handling for invalid permissions
- **Steps**:
  1. Login as staff without specific permission
  2. Try to access restricted page
- **Expected Result**: 
  - Access denied message displayed
  - Redirected to appropriate page
- **Priority**: High

### **TC-ERROR-002: Session Timeout**
- **Objective**: Verify session timeout handling
- **Steps**:
  1. Login as admin
  2. Wait for session to expire
  3. Try to perform action
- **Expected Result**: Redirected to login page
- **Priority**: Medium

### **TC-ERROR-003: Database Connection Error**
- **Objective**: Verify graceful handling of database errors
- **Steps**:
  1. Simulate database connection failure
  2. Try to access admin pages
- **Expected Result**: Error page displayed with appropriate message
- **Priority**: Low

---

## 🔄 **Integration Tests**

### **TC-INT-001: Admin Workflow**
- **Objective**: Verify complete admin workflow
- **Steps**:
  1. Login as admin
  2. Add new product
  3. Process new order
  4. Update user role
  5. Generate report
- **Expected Result**: All operations complete successfully
- **Priority**: High

### **TC-INT-002: Staff Workflow**
- **Objective**: Verify complete staff workflow
- **Steps**:
  1. Login as staff
  2. View assigned tasks
  3. Process orders
  4. Update product inventory
- **Expected Result**: All permitted operations complete successfully
- **Priority**: High

---

## 📱 **Responsive Design Tests**

### **TC-RESP-001: Mobile Admin Dashboard**
- **Objective**: Verify admin dashboard works on mobile
- **Steps**:
  1. Access admin dashboard on mobile device
  2. Test all interactive elements
- **Expected Result**: Dashboard responsive and functional
- **Priority**: Medium

### **TC-RESP-002: Tablet Admin Interface**
- **Objective**: Verify admin interface works on tablet
- **Steps**:
  1. Access admin interface on tablet
  2. Test navigation and forms
- **Expected Result**: Interface optimized for tablet
- **Priority**: Low

---

## 🎯 **Performance Tests**

### **TC-PERF-001: Dashboard Load Time**
- **Objective**: Verify dashboard loads within acceptable time
- **Steps**:
  1. Login as admin
  2. Measure dashboard load time
- **Expected Result**: Loads within 3 seconds
- **Priority**: Medium

### **TC-PERF-002: Large Dataset Handling**
- **Objective**: Verify system handles large datasets
- **Steps**:
  1. Create 1000+ users
  2. Create 1000+ products
  3. Test admin pages performance
- **Expected Result**: Pages load efficiently with pagination
- **Priority**: Low

---

## ✅ **Test Execution Checklist**

### **Pre-Test Setup**
- [ ] Test environment configured
- [ ] Test data prepared
- [ ] Admin account created
- [ ] Staff account created
- [ ] Regular user account created

### **Authentication Tests**
- [ ] TC-AUTH-001: Admin Login
- [ ] TC-AUTH-002: Staff Login
- [ ] TC-AUTH-003: Regular User Login
- [ ] TC-AUTH-004: Unauthorized Access Prevention

### **Dashboard Tests**
- [ ] TC-DASH-001: Admin Dashboard Access
- [ ] TC-DASH-002: Staff Dashboard Access
- [ ] TC-DASH-003: Dashboard Data Accuracy

### **User Management Tests**
- [ ] TC-USER-001: View Users List
- [ ] TC-USER-002: Edit User Role
- [ ] TC-USER-003: Deactivate User
- [ ] TC-USER-004: Staff Permission Restriction

### **Product Management Tests**
- [ ] TC-PROD-001: View Products List
- [ ] TC-PROD-002: Add New Product
- [ ] TC-PROD-003: Edit Product
- [ ] TC-PROD-004: Staff Product Management

### **Order Management Tests**
- [ ] TC-ORDER-001: View Orders List
- [ ] TC-ORDER-002: Update Order Status
- [ ] TC-ORDER-003: Staff Order Processing

### **Permission Management Tests**
- [ ] TC-PERM-001: View Permissions
- [ ] TC-PERM-002: Assign Role Permissions
- [ ] TC-PERM-003: Staff Permission Inheritance

### **Reports Tests**
- [ ] TC-REPORT-001: View Sales Reports
- [ ] TC-REPORT-002: Generate Custom Reports

### **Error Handling Tests**
- [ ] TC-ERROR-001: Invalid Permission Access
- [ ] TC-ERROR-002: Session Timeout
- [ ] TC-ERROR-003: Database Connection Error

### **Integration Tests**
- [ ] TC-INT-001: Admin Workflow
- [ ] TC-INT-002: Staff Workflow

### **Responsive Design Tests**
- [ ] TC-RESP-001: Mobile Admin Dashboard
- [ ] TC-RESP-002: Tablet Admin Interface

### **Performance Tests**
- [ ] TC-PERF-001: Dashboard Load Time
- [ ] TC-PERF-002: Large Dataset Handling

---

## 📊 **Test Results Summary**

### **Priority Distribution**
- **Critical**: 1 test case
- **High**: 15 test cases
- **Medium**: 8 test cases
- **Low**: 4 test cases

### **Total Test Cases**: 28

### **Estimated Execution Time**: 4-6 hours

### **Success Criteria**: 95% pass rate for High and Critical priority tests
