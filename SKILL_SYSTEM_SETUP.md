# SKILL SYSTEM - SETUP GUIDE

## 🎮 Triển khai hệ thống 4 Active Skill Slots (U, I, O, P)

### **BƯỚC 0: Tìm hiểu luồng hoạt động**

```
Người chơi:
├─ Tích lũy Damage/Distance → Stats change
│  └─ PlayerStats.OnStatsChanged fired
│     └─ SkillManager.CheckAllSkillsUnlock()
│        └─ Skill đủ điều kiện unlock
│           └─ Auto-assign vào slot trống
│              └─ SkillHotbarUI cập nhật
│
└─ Click vào slot UI → Mở Assignment Panel
   ├─ Chọn skill từ list → Gán vào slot
   └─ Hoặc bấm U/I/O/P để dùng skill
      └─ SkillSlotManager.TryUseSkill()
         └─ Cooldown bắt đầu
            └─ UI cập nhật cooldown
```

### **BƯỚC 1: Thêm Component vào Player**
1. Open Player prefab (`Assets/Prefabs/Player.prefab`)
2. Inspector → Add Component → SkillSlotManager
3. Drag `PlayerInputaction` InputActionAsset vào field "actions"
4. Drag ProjectileSpawnPoint vào field (hoặc để mặc định)

### **BƯỚC 2: Tạo Active Skills (ScriptableObjects)**

Tạo 4 skill asset vào `Assets/Resource/Skill/Active/`:

**1. Fireball.asset**
- Right-click → Create → Skill System/Active Skills/Fireball
- Skill Name: "Fireball"
- Type: Active
- Icon: (chọn sprite lửa)
- Cooldown: 0.8
- Description: "Bắn tia lửa về phía trước | Damage +20%"
- Drag fireballProjectilePrefab vào field
- Drag projectileSpawnPoint vào field

**2. Ice Spike.asset**
- Create → Skill System/Active Skills/Ice Spike
- Skill Name: "Ice Spike"
- Type: Active
- Icon: (chọn sprite băng)
- Cooldown: 1.5
- Description: "Triệu hồi 3 cây băng xung quanh | Defense +10%"
- Drag iceSpikePrefab vào field

**3. Dash Strike.asset**
- Create → Skill System/Active Skills/Dash Strike
- Skill Name: "Dash Strike"
- Type: Active
- Icon: (chọn sprite dash)
- Cooldown: 2.0
- Description: "Lao tới và đánh mạnh | Speed +30%"

**4. Heal.asset**
- Create → Skill System/Active Skills/Heal
- Skill Name: "Heal"
- Type: Active
- Icon: (chọn sprite heal)
- Cooldown: 3.0
- Description: "Phục hồi 30 HP | Health +5%"
- Drag healEffectPrefab vào field (optional)

**5. Slash Projectile.asset** (SkillK managed)
- Create → Skill System/Active Skills/Slash Projectile
- Skill Name: "Slash Shot"
- Type: Active
- Icon: (chọn sprite kiếm)
- Cooldown: 0.5
- Description: "Bắn tia kiếm khí"
- Drag slashProjectilePrefab vào field
- Drag projectileSpawnPoint vào field

### **BƯỚC 3: Thêm Skills vào SkillManager**
1. Player prefab → SkillManager component
2. All Skills list → Add 5 skills vừa tạo (active skills)
3. Cũng tạo/thêm passive skills (Traveler, BigHand)

### **BƯỚC 4: Tạo UI Hotbar**

1. **Tạo Canvas:**
   - Hierarchy → Right-click → UI → Canvas
   - Rename: "SkillHotbar"
   - Anchor Preset: Bottom-Left
   - Position: (0, 0)

2. **Tạo 4 Skill Slot Panels:**
   - Trong Canvas → Right-click → UI → Panel
   - Rename: "SlotU"
   - Add GridLayout hoặc arrange manually
   - Size: 80x80px each

3. **Cho mỗi Slot thêm:**
   - Image component (skill icon)
   - Text component (skill name)
   - Text component x2 (hotkey + cooldown)
   - **IMPORTANT:** Button component (để click mở panel gán skill)

4. **Add Script:**
   - Canvas → Add Component → SkillHotbarUI
   - Slot UIs array: 4 elements
   - Assign các UI elements từng slot

### **BƯỚC 5: Tạo Assignment Panel** (NEW!)

1. **Tạo Canvas mới hoặc Panel trên Canvas hiện tại:**
   - Hierarchy → Right-click → UI → Panel
   - Rename: "SkillAssignmentPanel"
   - Anchor: Center
   - Size: 600x400px
   - Màu background với alpha ~0.9

2. **Thêm các UI elements:**
   ```
   SkillAssignmentPanel
   ├─ Title (TextMeshProUGUI): "Chọn Kỹ Năng"
   ├─ Instructions (TextMeshProUGUI): "Chọn kỹ năng cho slot..."
   ├─ ScrollView (hiển thị list skills)
   │  └─ Content → dùng để spawn skill buttons
   └─ CloseButton (Button)
   ```

3. **Thêm Component:**
   - SkillAssignmentPanel → Add Component → SkillAssignmentPanel (script)
   - Skill Button Prefab: (tạo prefab Button với Image + Text)
   - Skill List Container: Reference to "Content" nội Content/ScrollView
   - Instruction Text: Reference to TextMeshProUGUI "Instructions"

4. **Tạo Skill Button Prefab:**
   - UI → Button
   - Rename: "SkillButtonPrefab"
   - Add Image (cho icon)
   - Add TextMeshProUGUI
   - Prefab này → đặt vào "Assets/Prefabs/UI/"
   - Assign vào SkillAssignmentPanel script

### **BƯỚC 5: Config Unlock Conditions** (Optional)
- Muốn một số active skill unlock tự động:
  - Kéo LevelCondition hoặc DamageCondition vào unlockConditions
  - Set điều kiện (level > 5, damage > 50)

### **BƯỚC 6: Test Game**

```
Chơi game → Kiểm tra:
1. Bấm J để tấn công → Tích lũy Damage
2. Khi Damage > 50 → 1 skill tự động gán vào slot trống ✓
3. Bấm U/I/O/P → Skill activate với cooldown ✓
4. Click vào một slot UI → Mở Assignment Panel ✓
5. Chọn skill khác từ panel → Gán vào slot ✓
6. UI cập nhật icon + cooldown timer ✓
7. Passive skill unlock → Speed thay đổi ✓
8. Heal skill restore HP ✓
```

## 🔧 Cách hoạt động

### **Auto-Assign Flow:**
1. Khi skill unlock → `SkillManager.OnSkillUnlocked` fired
2. Nếu active skill → `AutoAssignSkillToEmptySlot()` gọi
3. Tìm slot trống → Gán skill
4. `OnSkillSlotChanged` event → UI update

### **Manual Assign Flow:**
1. Click vào một slot UI
2. `SkillHotbarUI.OnSlotClicked()` mở Assignment Panel
3. Panel hiển thị list unlocked active skills
4. Chọn skill → `SelectSkillAssignment()`
5. `SkillSlotManager.AssignSkillToSlot()` gán
6. UI cập nhật

### **Skill Usage Flow:**
1. User bấm U/I/O/P
2. InputAction triggered → `SkillSlotManager.TryUseSkill()`
3. Check: Dead? Dashing? On cooldown?
4. `SkillSO.Activate()` → Skill effect
5. Cooldown started
6. UI cập nhật cooldown bar

## 🔧 Troubleshooting

| Vấn đề | Giải pháp |
|--------|----------|
| Skills không activate | Check InputActionAsset assigned + Enable() được gọi |
| Cooldown không update | Check SkillSlotManager.Update() running |
| UI hotbar không hiển thị | Check SkillHotbarUI refs assigned + Canvas enabled |
| Assignment Panel không mở | Check SkillAssignmentPanel linked + Button onClick setup |
| Auto-assign không hoạt động | Check SkillManager có skillSlotManager reference |
| Passive effects không apply | Check PassiveSkillEffectSystem enabled |
| Skills không appear trong list | Check skills marked as Active + unlocked + in SkillManager |

## 📁 File Structure

```
Assets/
├── Scripts/Player/Skill/
│   ├── SkillManager.cs ✏️ (updated: AddAutoAssign + GetUnlockedActiveSkills)
│   ├── SkillSO.cs ✓
│   ├── SkillSlotManager.cs ✓
│   ├── PassiveSkillEffectSystem.cs ✓
│   └── Skills/
│       ├── FireballSkill.cs
│       ├── IceSpikeSkill.cs
│       ├── DashStrikeSkill.cs
│       ├── HealSkill.cs
│       └── SlashProjectileSkill.cs
│
├── Scripts/UI/
│   ├── SkillHotbarUI.cs ✏️ (updated: CloseButton + OnSlotClicked)
│   └── SkillAssignmentPanel.cs ✨ NEW
│
├── Prefabs/UI/
│   └── SkillButtonPrefab.prefab ✨ CREATE THIS
│
├── Resource/Skill/
│   └── Active/ 
│       ├── Fireball.asset
│       ├── Ice Spike.asset
│       ├── Heal.asset
│       └── ...
│
└── PlayerInputaction.inputactions ✓ (U, I, O, P added)
```

## ✨ **Novo: Funcionalidades adicionadas**

1. **Auto-Assign Sistema**
   - Quando skill ativa é desbloqueada → Automaticamente atribui ao primeiro slot vazio
   - Suporta até 4 skills ao mesmo tempo
   - Método: `SkillManager.AutoAssignSkillToEmptySlot(skill)`

2. **Manual Skill Selection**
   - UI Panel com lista de skills desbloqueados
   - Click em slot → Abre panel
   - Seleciona skill → Atribui imediatamente
   - Botão "Remover" para limpar slot

3. **Dynamic Update**
   - UI atualiza quando novo skill é desbloqueado
   - Panel mostra apenas skills desbloqueados
   - Cross-reference entre hotbar e assignment panel

## 🎯 Next Steps
1. ✅ Scripts triển khai - Done
2. 📝 Create Skills (5 assets) - You do this
3. 🎨 Create/Setup UI Hotbar - You do this
4. 🧪 Test in Play mode - You do this
