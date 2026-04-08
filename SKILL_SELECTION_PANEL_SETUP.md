# 🎯 SKILL SELECTION PANEL - SETUP GUIDE

## O que foi adicionado

**SkillSelectionPanel.cs** - Novo component que cria UI panel interativo para:
- Abrindo quando clica em um slot
- Mostrando lista de skills desbloqueados
- Permitindo escolher qual skill colocar no slot
- Fechar quando seleciona ou clica "Fechar"

---

## ⚙️ SETUP NO UNITY EDITOR

### **Passo 1: Criar UI Panel** 

```
Canvas (existente)
├─ SkillHotbar (existente)
└─ SkillSelectionPanel (NOVO Panel)
   ├─ Anchor: Center
   ├─ Size: 600x500
   ├─ Active: OFF (fica desativado até abrir)
   └─ Cor de fundo: preto com alpha ~0.7 (para blur effect)
```

### **Passo 2: Adicionar UI Elements**

Dentro de `SkillSelectionPanel`:

```
Panel (SkillSelectionPanel)
├─ Title (TextMeshProUGUI)
│  ├─ Text: "Chọn Kỹ Năng"
│  ├─ Font Size: 36
│  └─ Alignment: Top-Center
│
├─ Instruction (TextMeshProUGUI)
│  ├─ Text: "Chọn một kỹ năng:"
│  ├─ Font Size: 24
│  └─ Color: Gray
│
├─ ScrollView
│  ├─ Horizontal Scroll: OFF
│  ├─ Vertical Scroll: ON
│  ├─ Size: 550x350
│  └─ Content (empty) → Set this as skillListContainer
│
└─ CloseButton (Button)
   ├─ Text: [ Đóng ]
   ├─ Size: 100x50
   ├─ Position: Bottom-Center
   └─ Color: Red
```

### **Passo 3: Tạo Skill Button Prefab**

```
UI → Button - TextMeshProUGUI
├─ Rename: "SkillButtonPrefab"
├─ Size: 500x70
├─ Layout:
│  ├─ Image (skill icon) - Left side
│  ├─ TextMeshProUGUI (skill name) - Right side
└─ Prefab này → Assets/Prefabs/UI/SkillButtonPrefab.prefab
```

### **Passo 4: Add Component e Configure**

Na `SkillSelectionPanel`:

```
Add Component → SkillSelectionPanel

Inspector:
├─ Skill Button Prefab: [Drag SkillButtonPrefab aqui]
├─ Skill List Container: [Drag Content do ScrollView]
├─ Title Text: [Drag "Title" TextMeshProUGUI]
├─ Instruction Text: [Drag "Instruction" TextMeshProUGUI]
└─ Close Button: [Drag "CloseButton" Button]
```

### **Passo 5: Link Panel ao Hotbar**

Em `SkillHotbarUI`:

```
Inspector → SkillHotbarUI
├─ Skill Slot Manager: (auto-find)
├─ Skill Manager: (auto-find)
└─ Skill Selection Panel: [Drag SkillSelectionPanel aqui]
```

---

## 🎮 COMO FUNCIONA

### **Fluxo de Uso:**

```
1. Jogador clica em um slot (U/I/O/P)
   ↓
2. OnSlotClicked() chamado
   ↓
3. Se SkillSelectionPanel existe:
   → Panel abre
   → Mostra título: "Chọn Kỹ Năng cho Slot U"
   → Lista todos os skills desbloqueados
   → Button "[ Không có ]" para remover

4. Jogador clica em um skill (ou "Không có")
   ↓
5. SkillSlotManager.AssignSkillToSlot() chamado
   ↓
6. Panel fecha
   ↓
7. SkillHotbarUI atualiza com novo icon
```

### **Fallback (se panel não existir):**
- Rotaciona entre skills (comportamento antigo)

---

## 🧪 TEST

**Play game:**
```
1. Click em um slot (U/I/O/P)
   ✓ Deve abrir panel com título "Chọn Kỹ Năng cho Slot U/I/O/P"

2. Veja lista com skills desbloqueados
   ✓ Se 2 skills desbloqueados, mostra 3 buttons (2 skills + 1 "Không có")

3. Click em um skill
   ✓ Panel fecha
   ✓ Hotbar atualiza com novo icon
   ✓ Console: "[SkillSelectionPanel] Gán 'FireBall' vào slot 0"

4. Pressione U/I/O/P
   ✓ Skill ativa com novo skill!
```

---

## 📋 INSPECTOR CHECKLIST

```
☑ SkillSelectionPanel Component
  ☑ Skill Button Prefab → SkillButtonPrefab
  ☑ Skill List Container → Content
  ☑ Title Text → Title TextMeshProUGUI
  ☑ Instruction Text → Instruction TextMeshProUGUI
  ☑ Close Button → CloseButton Button

☑ SkillHotbarUI
  ☑ Skill Selection Panel → SkillSelectionPanel

☑ UI Hierarchy
  ☑ Canvas
    ☑ SkillHotbar
       ☑ SlotU (Button)
       ☑ SlotI (Button)
       ☑ SlotO (Button)
       ☑ SlotP (Button)
    ☑ SkillSelectionPanel (Panel) → starts OFF
       ☑ Title
       ☑ Instruction
       ☑ ScrollView
          ☑ Content (empty - skills spawn aqui)
       ☑ CloseButton
```

---

## 🐛 TROUBLESHOOTING

### Problem: Panel não abre

**Solução:**
1. Verificar se `SkillSelectionPanel` foi add ao Player prefab (não, está no Canvas!)
2. Verificar se field `skillSelectionPanel` no SkillHotbarUI está assigned
3. Verificar se panel está no hierarchy (não Off)
4. Console deve mostrar: `[SkillHotbarUI] Mở selection panel cho slot 0`

### Problem: Buttons não aparecem

**Solução:**
1. Verificar se `SkillButtonPrefab` está assignado
2. Verificar se `skillListContainer` aponta para Content correto
3. Verificar se há skills desbloqueados (se não, console mostra: "Chưa có kỹ năng")

### Problem: Click em skill não fecha panel

**Solução:**
1. Verificar se `OnSkillSelected()` foi chamado
2. Verificar se `CloseButton` foi assigned
3. Console deve mostrar logs

---

## 💡 OPCIONAIS

### Dark overlay para blur background

```
Panel (SkillSelectionPanel)
├─ Image component
├─ Source Image: Branco
├─ Color: Preto com alpha 0.5
├─ Size: Screen size
└─ Layout Group: LayoutElement para cobrir tela
```

### Animação ao abrir/fechar

```csharp
// Adicionar no Start() de SkillSelectionPanel:
GetComponent<CanvasGroup>().alpha = 0;

// Na OpenPanelForSlot():
StartCoroutine(FadeIn());

// Na ClosePanel():
StartCoroutine(FadeOut());
```

### Sound effect

```csharp
// Na OnSkillSelected():
AudioSource.PlayClipAtPoint(selectSound, transform.position);
```
