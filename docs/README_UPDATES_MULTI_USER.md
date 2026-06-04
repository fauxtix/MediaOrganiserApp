# README Updates - Multi-User Architecture Documentation

**Date:** 2026-01-XX  
**Purpose:** Updated README.md to accurately reflect the multi-user architecture with role-based data isolation

---

## Changes Made

### 1. **Title and Positioning** (Lines 1-16)
**Before:**
- Title: "Media Organiser App"
- Generic description about organizing media

**After:**
- Title: "Media Organiser App"
- **Subtitle: "Multi-User Collection Management System"**
- Explicitly states it's a **multi-user application**
- Highlights: **role-based access control** and **user-specific data isolation**
- Added use case examples: families, small libraries, archive projects, collector teams

---

### 2. **Key Features Section** (Line ~91)
**Replaced:** Generic "User Management & Authorization" section

**With:** "🔐 Multi-User System & Data Isolation" featuring:
- Multi-user architecture explanation
- **Role-based data access** breakdown:
  - **Admin Role**: Views ALL users' data + full system access
  - **GeneralUser Role**: Views ONLY their own data (filtered by UserID)
- User management capabilities (Admin-only)
- **Data isolation** mechanism explanation
- **Real-world use cases** with examples

---

### 3. **User Roles & Data Isolation Section** (Lines ~352-419)
**Completely rewrote** this section based on actual code implementation:

#### Replaced:
- Incorrect three-role model (Admin, Editor, Viewer)
- Generic permission descriptions

#### With:
- **Accurate two-role model** from codebase:
  - `UserRole.Admin`
  - `UserRole.GeneralUser`

#### Added:
- **Data Access Comparison Table** showing data scope differences
- **Code Example** from `PublicacoesRepository.cs` demonstrating query filtering:
  ```csharp
  if (userRole == UserRole.GeneralUser)
  {
	  sbData.Append(" WHERE IdFormato = @IdFormato AND UserID = @UserID");
	  // Returns only this user's publications
  }
  else // Admin
  {
	  sbData.Append(" WHERE IdFormato = @IdFormato ");
	  // Returns ALL publications from all users
  }
  ```

- **"How Data Isolation Works"** section explaining:
  - UserID tagging on entities
  - Repository-level filtering
  - Security enforcement (not just UI-level)

- **Detailed role breakdowns**:
  - **GeneralUser capabilities and restrictions**
  - **Admin capabilities and system control features**
  - "Perfect for" use case examples for each role

---

## Technical Accuracy

### Evidence from Codebase:

1. **Entity-level UserID tracking:**
   - `Publicacao.cs`: `int UserID { get; set; }`
   - `Media.cs`: `string CreatedBy { get; set; }`

2. **SessionParameters tracking:**
   ```csharp
   public static int UserID { get; set; }
   public static UserRole UserRole { get; set; }
   ```

3. **Repository-level filtering:**
   - All queries in `PublicacoesRepository.cs` check `userRole`
   - GeneralUser queries append `AND UserID = @UserID`
   - Admin queries omit UserID filter (sees all data)

4. **Enum definition:**
   ```csharp
   public enum UserRole
   {
	   Admin,
	   GeneralUser
   }
   ```

5. **User Management UI:**
   - `ManageUsers.razor`: `@attribute [Authorize(Roles = "Administrator")]`
   - Admin-only access to user CRUD operations

---

## Impact

### Benefits:
✅ **Accurate representation** of the system's actual capabilities  
✅ **Clear positioning** as a multi-user system (not just "personal")  
✅ **Better marketing** for target audiences (families, small teams, archives)  
✅ **Technical clarity** with code examples showing data isolation  
✅ **Developer-friendly** documentation with implementation details  

### Removed Confusion:
❌ Removed incorrect "Editor" and "Viewer" roles (not implemented)  
❌ Removed vague "role-based access control" without specifics  
❌ Corrected misleading "personal" positioning  

---

## Next Steps (Optional Enhancements)

1. **Add Mermaid diagram** showing user role data flow
2. **Add screenshots** of:
   - Admin viewing all users' data
   - GeneralUser viewing only their data
   - Manage Users page
3. **Update Web.Blazor/README.md** with same corrections
4. **Update README-PT.md** (Portuguese version) with same changes
5. **Add quick-start guide** for setting up first Admin and GeneralUser accounts

---

## Files Modified

- ✅ `README.md` (3 replacements applied successfully)

## Build Status

✅ **Build successful** - No compilation errors introduced

---

## Verification Checklist

- [x] Verified role enum in codebase (`UserRole.Admin`, `UserRole.GeneralUser`)
- [x] Confirmed data filtering in repositories (`PublicacoesRepository.cs`)
- [x] Verified SessionParameters usage for user context
- [x] Confirmed entity-level UserID fields (`Publicacao`, `Media`)
- [x] Checked admin-only pages (`ManageUsers.razor` with `[Authorize(Roles = "Administrator")]`)
- [x] Build passes with no errors
- [x] README accurately reflects implementation

---

**Conclusion:** The README now accurately documents the multi-user architecture with role-based data isolation as implemented in the codebase. The app is positioned correctly as a "Multi-User Collection Management System" suitable for personal, family, and small organizational use.
