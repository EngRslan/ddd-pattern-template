# Role-Based Directives

This directory contains Angular structural directives for role-based access control in the UI.

## Available Directives

### 1. `*appHasRole`
Shows the element only if the user has the specified role.

```html
<!-- Single role -->
<div *appHasRole="'Admin'">
  Only visible to users with Admin role
</div>

<!-- Multiple roles (user must have at least one) -->
<div *appHasRole="['Admin', 'Manager']">
  Visible to users with Admin OR Manager role
</div>
```

### 2. `*appHasNotRole`
Shows the element only if the user does NOT have the specified role(s).

```html
<!-- Hide from Admin users -->
<div *appHasNotRole="'Admin'">
  Not visible to Admin users
</div>

<!-- Hide from Admin and Manager users -->
<div *appHasNotRole="['Admin', 'Manager']">
  Not visible to Admin or Manager users
</div>
```

### 3. `*appHasAnyRole`
Shows the element if the user has ANY of the specified roles.

```html
<!-- Show if user has any role -->
<div *appHasAnyRole="[]">
  Visible to any authenticated user with roles
</div>

<!-- Show if user has any of these roles -->
<div *appHasAnyRole="['Admin', 'Manager', 'User']">
  Visible if user has Admin OR Manager OR User role
</div>
```

### 4. `*appHasAllRoles`
Shows the element only if the user has ALL of the specified roles.

```html
<!-- User must have both Admin AND Manager roles -->
<div *appHasAllRoles="['Admin', 'Manager']">
  Only visible to users with both Admin AND Manager roles
</div>
```

## Usage Examples

### Dashboard Features
```html
<!-- Admin-only section -->
<div class="admin-panel" *appHasRole="'Admin'">
  <h3>Admin Controls</h3>
  <button>Manage Users</button>
  <button>System Settings</button>
</div>

<!-- Manager features -->
<div class="manager-tools" *appHasAnyRole="['Admin', 'Manager']">
  <h3>Management Tools</h3>
  <button>View Reports</button>
  <button>Export Data</button>
</div>

<!-- Regular user content -->
<div class="user-content" *appHasNotRole="['Admin', 'Manager']">
  <p>Welcome to the user dashboard!</p>
</div>
```

### Navigation Menu
```html
<nav>
  <a routerLink="/home">Home</a>
  <a routerLink="/profile">Profile</a>
  <a routerLink="/admin" *appHasRole="'Admin'">Admin Panel</a>
  <a routerLink="/reports" *appHasAnyRole="['Admin', 'Manager']">Reports</a>
  <a routerLink="/settings" *appHasAllRoles="['Admin', 'SuperUser']">Advanced Settings</a>
</nav>
```

### Conditional Actions
```html
<div class="user-card">
  <h4>{{ user.name }}</h4>
  <p>{{ user.email }}</p>
  
  <!-- Edit button for admins and managers -->
  <button *appHasAnyRole="['Admin', 'Manager']" (click)="editUser(user)">
    Edit
  </button>
  
  <!-- Delete button for admins only -->
  <button *appHasRole="'Admin'" (click)="deleteUser(user)">
    Delete
  </button>
  
  <!-- View details for all authenticated users -->
  <button *appHasAnyRole="[]" (click)="viewDetails(user)">
    View Details
  </button>
</div>
```

### Form Fields
```html
<form>
  <input type="text" placeholder="Name" [(ngModel)]="name">
  <input type="email" placeholder="Email" [(ngModel)]="email">
  
  <!-- Role selection for admins only -->
  <select *appHasRole="'Admin'" [(ngModel)]="selectedRole">
    <option value="User">User</option>
    <option value="Manager">Manager</option>
    <option value="Admin">Admin</option>
  </select>
  
  <!-- Department field for managers and above -->
  <input *appHasAnyRole="['Admin', 'Manager']" 
         type="text" 
         placeholder="Department" 
         [(ngModel)]="department">
</form>
```

## Implementation Notes

1. **Role Extraction**: The directives automatically check for roles in multiple locations:
   - `user.profile.role` / `user.profile.roles`
   - `user.role` / `user.roles`
   - Supports both single string and array formats

2. **Reactive Updates**: The directives automatically update when the user's authentication state changes.

3. **Performance**: Uses Angular's structural directive approach for optimal performance - elements are completely removed from the DOM when conditions aren't met.

4. **Null Safety**: Handles cases where user is not authenticated gracefully.

## Common Role Names

Typical role names used in applications:
- `Admin` - Full system access
- `Manager` - Management features access
- `User` - Standard user access
- `Guest` - Limited guest access
- `SuperUser` - Super admin privileges
- `Moderator` - Content moderation access
- `Editor` - Content editing capabilities
- `Viewer` - Read-only access

## Testing

To test these directives in development:

1. Mock different user roles in your AuthService
2. Use browser developer tools to modify the user object
3. Create test components with various role combinations