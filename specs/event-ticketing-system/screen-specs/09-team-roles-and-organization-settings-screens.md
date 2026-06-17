# Team, Roles And Organization Settings Screens

## Screen: Team Members

### Purpose

Cho Organization Owner quản lý ai được truy cập organizer workspace và mỗi người có quyền gì.

### Wireframe

```text
┌──────────────────────────────────────────────────────────────┐
│ Team                                         [Invite member] │
│ [Search member...] [Role v] [Status v]                       │
├──────────────────────────────────────────────────────────────┤
│ Name              Email              Role             Status │
│ Mai Nguyen        mai@eventbox.com    Event Manager    Active │
│ Linh Tran         linh@eventbox.com   Check-in Lead    Active │
│ An Pham           an@eventbox.com     Pending invite   Pending│
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| `Invite member` | Opens Invite Member modal. Visible only to Organization Owner and Platform Admin. |
| Search | Searches by name and email. Debounce 300 ms. |
| Role filter | Values: All, Organization Owner, Event Manager, Registration Manager, Finance Manager, Check-in Lead, Check-in Operator, Viewer. |
| Status filter | Values: All, Active, Pending, Disabled. |
| Member row | Opens Member Detail drawer. |
| Row `...` | Menu: Edit roles, Resend invite, Disable access, Remove from organization. |

### Empty States

- No members except owner: show `Invite your team to help manage events.` + `Invite member`.
- Filtered empty: show `No members match your filters.` + `Clear filters`.

## Modal: Invite Member

### Wireframe

```text
┌──────────────── Invite member ────────────────┐
│ Email *              [____________________]   │
│ Name                 [____________________]   │
│ Organization role    [Viewer v]               │
│ Event access         (• Specific events)       │
│                      ( ) All current events    │
│                      ( ) All current and future │
│ Events *             [Select events...]        │
│ Event role *          [Check-in Operator v]    │
│                                               │
│ [Cancel] [Send invite]                        │
└───────────────────────────────────────────────┘
```

### Inputs

| Input | Validation | Behavior |
| --- | --- | --- |
| Email | Required, valid email | If email already belongs to member, show existing member conflict. |
| Name | Optional | Used in invite email greeting. |
| Organization role | Required | Defaults to Viewer. Organization Owner can only be assigned by current Organization Owner. |
| Event access | Required | Controls whether event selector is shown. |
| Events | Required when specific events selected | Multi-select active/draft events. |
| Event role | Required when event access is selected | Defaults to Check-in Operator for onsite invites. |

### Buttons

- `Cancel`: closes modal; dirty modal asks confirm discard.
- `Send invite`: creates pending invite and sends email.

### Success Behavior

- Toast: `Invite sent to an@eventbox.com.`
- Pending member appears in Team table.

### Error Behavior

- Existing active member: `This email is already a team member. Edit their role instead.`
- Existing pending invite: show `Resend invite` action.

## Drawer: Member Detail

### Wireframe

```text
┌──────────────── Mai Nguyen ────────────────┐
│ mai@eventbox.com · Active                    │
│ Organization role                           │
│ [Event Manager v]                           │
│                                             │
│ Event access                                │
│ Eventbox Summit      Event Manager           │
│ AI Workshop         Viewer                  │
│                                             │
│ Recent activity                             │
│ Published Eventbox Summit · Jun 6, 10:12     │
│ Added seats · Jun 5, 16:20                  │
│                                             │
│ [Save changes] [Disable access]             │
└─────────────────────────────────────────────┘
```

### Behavior

- Changing role requires `Save changes`.
- Removing the last Organization Owner is blocked.
- Disabling access immediately signs out the member from organizer workspace.
- Any role change is audit logged.

## Screen: Organization Settings

### Purpose

Cho Organization Owner cấu hình thông tin organization và defaults dùng cho events mới.

### Wireframe

```text
┌──────────────── Organization settings ───────────────────────┐
│ Organization name *      [Eventbox Events________]             │
│ Public contact email *   [events@eventbox.com____]             │
│ Default timezone *       [Asia/Ho_Chi_Minh v]                 │
│ Default currency *       [VND v]                              │
│ Brand logo               [Upload] [Remove]                    │
│                                                               │
│ [Save changes]                                                │
└──────────────────────────────────────────────────────────────┘
```

### Controls

| Control | Behavior |
| --- | --- |
| Organization name | Required. Used in organizer workspace and public event attribution. |
| Public contact email | Required. Used as default event contact. |
| Default timezone | Required. Used when creating event. |
| Default currency | Required. Used as event currency default. |
| Brand logo | Optional upload. Validates file size/type. |
| Save changes | Enabled only when dirty and valid. |

### Acceptance Criteria

- Only Organization Owner and Platform Admin can access Team and Organization Settings.
- Last Organization Owner cannot be removed or downgraded.
- Role changes affect navigation immediately after refresh and at next authorization check.
- Invite links expire after 7 days.

