# ⚙️ TROUBLESHOOTING GUIDE - Skill System

## ❌ Problem 1: Bấm U/I/O/P không kích hoạt skill

### Triệu chứng:
- Bấm U/I/O/P nhưng không thấy skill được cast
- Console không hiển thị "[SkillSlotManager] TryUseSkill"

### Nguyên nhân:
- InputActions không được enable đúng cách
- PlayerController không được liên kết
- Actions Skill_U, I, O, P không tồn tại trong InputActionAsset

### Giải pháp:

**1. Debug với SkillInputDebugger:**
```
Player (Prefab)
  → Add Component → SkillInputDebugger
  → Assign InputActionAsset (PlayerInputaction)
  → Play game
  → Bấm U/I/O/P
  → Check Console logs
```

**Expected logs trên Console:**
```
[SkillInputDebugger] Skills input listeners attached - ready to test!
[SkillInputDebugger] Key U pressed! (Slot 0)
  → Skill: FireBall, Cooldown: 0.00s / 0.80s
```

**2. Nếu logs không xuất hiện:**

a) **Kiểm tra InputActionAsset:**
- Open: Assets/PlayerInputaction.inputactions
- Verify "Player" action map tồn tại
- Verify actions: Skill_U, Skill_I, Skill_O, Skill_P tồn tại
- Verify bindings: <Keyboard>/u, /i, /o, /p

b) **Kiểm tra Player setup:**
- PlayerController script có property `IsDead`, `IsDashing`?
- SkillSlotManager được add vào Player?
- InputActionAsset được assign?

c) **Xem logs SkillSlotManager:**
```
[SkillSlotManager] Skill_U action habilitada
[SkillSlotManager] Skill_I action habilitada
[SkillSlotManager] Skill_O action habilitada
[SkillSlotManager] Skill_P action habilitada
```
If not appearing → Actions không podem ser encontradas!

---

## ❌ Problem 2: Click slot UI không troca skill

### Triệu chứng:
- Clika em um slot mas nada acontece
- Nenhum log de "[SkillHotbarUI] Slot X clicked"

### Giải pháp:

**1. Verificar que SkillManager está conectado:**
```csharp
// Nos logs deverá aparecer:
[SkillHotbarUI] SkillManager não encontrado!
```
Se esse log não aparece, mas click não funciona:

**2. Verificar se há skills desbloqueados:**
- Play game
- Atuda a condição de unlock (ex: Damage > 100)
- Logs deverão aparecer:
```
Đã mở khóa kỹ năng: FireBall!
[SkillManager] Auto-assigned 'FireBall' to slot 0
```

**3. Se skills foram desbloqueados mas click ainda não funciona:**
- Check Button component está no parent do slot icon
- Check Input System não está em conflict

---

## ❌ Problem 3: Auto-assign não funciona

### Triệu chứng:
- Skill foi desbloqueado (log diz "Đã mở khóa")
- Mas não vê no hotbar

### Giải pháp:

**1. Verificar que skill está em allSkills:**
- Player prefab
- SkillManager component
- Verifique lista "allSkills" tem quantos itens?

**2. Verificar condição de unlock:**
```
Se for DamageCondition:
  → Check PlayerStats.damageDealt
  → Verifique threshold está correto

Se for DistanceCondition:
  → Check PlayerStats.distanceTraveled
  → Attack enemy para acumular damage
```

**3. Logs esperados quando skill desbloqueia:**
```
Đã mở khóa kỹ năng: FireBall!
Gán skill 'FireBall' vào slot 0 (U)
[SkillManager] Auto-assigned 'FireBall' to slot 0
```

---

## ❌ Problem 4: Skill ativa mas não faz nada

### Triệu chứng:
- Vê log: "[SkillSlotManager] ✓ Ativando skill 'FireBall'"
- Mas sem efeito visual ou dano

### Causa:
- SkillSO.Activate() não foi implementada
- Prefabs não foram assignados

### Giải pháp:

**1. Verificar implementação de Activate():**
```csharp
// FireballSkill.cs
public override void Activate(GameObject player)
{
    if (fireballPrefab == null)
    {
        Debug.LogError("FireballSkill: fireballPrefab não assignado!");
        return;
    }
    // ... resto do código
}
```

**2. Checklist para cada skill:**
- [ ] Script (ex: FireballSkill.cs) existe?
- [ ] Classe herda de SkillSO?
- [ ] Override Activate() está implementado?
- [ ] Prefabs necessários (fireballPrefab, spawnPoint) estão assignados?

**3. Test individual skill:**
```csharp
// No Inspector de qualquer skill asset:
Debug.Log($"Skill name: {skillName}");
Debug.Log($"Type: {type}");
Debug.Log($"Cooldown: {cooldown}");
```

---

## ✅ Quick Setup Checklist

```
[ ] Player prefab
    [ ] SkillSlotManager component
    [ ] InputActionAsset assigned
    [ ] SkillManager component
    [ ] 4+ skills em allSkills list

[ ] InputActionAsset (PlayerInputaction)
    [ ] Player action map exists
    [ ] Skill_U, I, O, P actions exist
    [ ] Keybindings: <Keyboard>/u, /i, /o, /p

[ ] Skill Assets
    [ ] Cada skill template criado (Fireball, IceSpike, etc)
    [ ] Type = Active
    [ ] Cooldown > 0
    [ ] Icon assigned (optional but recommended)
    [ ] Unlock conditions assigned
    [ ] Activate() implementado

[ ] UI
    [ ] Canvas com hotbar
    [ ] 4 slots (U, I, O, P)
    [ ] SkillHotbarUI script added
    [ ] SlotUI array configurada com 4 elementos

[ ] Test in Play Mode
    [ ] Novo skill desbloqueia automaticamente
    [ ] Aparece no hotbar
    [ ] Click muda skill
    [ ] Pressionar hotkey ativa skill
    [ ] Cooldown funciona
```

---

## 🔍 Debug Commands

**Para testar manualmente no Play Mode:**

1. **Enable skill manualmente:**
```unity
// Selecione Player→ SkillSlotManager
// Na janela do Debug, chame:
skillSlotManager.AssignSkillToSlot(0, fireballSkill); // seu skill
```

2. **Force skill unlock:**
```csharp
// PlayerStats component
// Call via Inspector:
playerStats.ForceTriggerStatsChange();
```

3. **Check state atual:**
```
Console → Filter por "[Skill"
```
Verá todos os logs do sistema de skill com [Skill prefix.

---

## 📞 Última Resort: Enable All Logs

Se ainda está falhando, ative o máximo de logging:

1. Abra Console window (Window → General → Console)
2. Set filter a "SkillIs"
3. Clique no slot UI
4. Pressione uma tecla (U/I/O/P)
5. Screenshot ou copia os ENTIRE logs
6. Procure por patterns:
   - Qualquer **ERROR** (in red)
   - **WARNING** logs (in yellow)
   - Cheque se **TryUseSkill** foi chamado
