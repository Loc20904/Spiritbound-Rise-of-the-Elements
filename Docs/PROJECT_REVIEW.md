# Đánh giá Spiritbound — 24/10/2026

> Cập nhật 26/10/2026: xem [báo cáo rework](REWORK_2026-10-26.md). Các nhận xét dưới đây ghi nhận hiện trạng trước khi sửa.

Đánh giá dựa trên mã nguồn và cấu hình sau khi sắp xếp thư mục. Đã đối chiếu các luồng HP, combat, biến hình, Tarot, save/load, inventory, pause, settings và các thành phần gắn trên prefab player. Dự án có 118 file C# với khoảng 11.902 dòng. Đây là phân tích tĩnh, chưa tái hiện các tình huống dưới đây trong Play Mode và chưa đo hiệu năng.

Đợt tái cấu trúc trước đã kiểm tra mở 11 scene build và tải 112 prefab mà không có Missing Script. Kết quả đó xác nhận liên kết asset; khả năng hoạt động của gameplay cần các bước kiểm tra riêng bên dưới.

## Các vấn đề cần ưu tiên

### R01 — Cao: HP, giáp, chết và hồi sinh đang có hai chủ sở hữu

**Bằng chứng:** `PlayerStats.TakeDamage` trừ giáp rồi gọi `PlayerHealth.TakeDamage`, nơi giáp bị trừ thêm lần nữa. Prefab `Player/Player.prefab` và `Player/1 - Idle_0.prefab` đều gắn cả hai component. Cả hai cũng có cờ chết, coroutine hồi sinh và respawn point riêng.

Ví dụ với damage 20 và giáp 5 ở cả hai component: đường gọi qua PlayerStats còn 10 damage; đường gọi trực tiếp PlayerHealth còn 15. Nhiều chiêu boss gọi PlayerStats trong khi quái thường gọi PlayerHealth, nên cùng một thông số damage cho kết quả khác nhau.

**Đề xuất:** giữ một nơi duy nhất tính damage/giáp và điều khiển vòng đời HP/chết/hồi sinh. Tách cấu hình chỉ số ra khỏi trạng thái runtime và thông báo UI bằng sự kiện.

**Nguồn:** [PlayerStats.cs](../Assets/_Project/Scripts/Player/PlayerStats.cs#L48), [PlayerHealth.cs](../Assets/_Project/Scripts/Player/PlayerHealth.cs#L52).

**Kiểm tra:** damage qua mọi nguồn phải có cùng quy tắc; một lần chết chỉ khởi động một luồng hồi sinh.

### R02 — Cao: Knockback có thể hủy coroutine hồi sinh

**Bằng chứng:** `PlayerHealth.ApplyKnockback` gọi `StopAllCoroutines()`. `DirtBullet`, đạn hệ Đất và một số đòn cận chiến gọi TakeDamage rồi tiếp tục ApplyKnockback mà không kiểm tra cú đánh có gây chết hay không. Nếu TakeDamage vừa khởi động RespawnRoutine trên PlayerHealth, coroutine đó sẽ bị hủy; các cờ dead/respawning có thể không được trả lại.

PlayerStats hiện có luồng hồi sinh thứ hai nên triệu chứng thực tế còn phụ thuộc component đang gắn: có thể kẹt chết hoặc hồi sinh rồi không nhận damage tiếp. Cần tái hiện thay vì coi việc nhân vật xuất hiện lại là đủ.

**Đề xuất:** giữ handle riêng cho coroutine knockback, ngăn knockback khi chết và quản lý hủy hành động theo trạng thái.

**Nguồn:** [PlayerHealth.cs](../Assets/_Project/Scripts/Player/PlayerHealth.cs#L93), [DirtBullet.cs](../Assets/_Project/Scripts/Enemy/Earth/Flying/DirtBullet.cs#L41), [ArrowBullet.cs](../Assets/_Project/Scripts/Enemy/Fire/Ranged/ArrowBullet.cs#L74).

**Kiểm tra:** để đạn hệ Đất kết liễu player, chờ hồi sinh rồi nhận thêm một cú đánh.

### R03 — Cao: Save/load chưa khôi phục đầy đủ tiến trình

**Bằng chứng:** SaveData chỉ có X/Y, HP và tên scene. Dòng khôi phục vị trí trong `PlayerHealth.LoadPlayer` đang bị comment. Hàm được gọi ở Start của mỗi player và áp HP từ save mà không kiểm tra scene hoặc lý do khởi tạo. Inventory, form, checkpoint và cờ tuyến truyện chưa có trong save.

`GameProgressManager` giữ cờ trong RAM qua DontDestroyOnLoad. `MainMenuManager.NewGame` chỉ xóa file save, LastScene và stat; chưa reset các cờ này. Nếu chọn Kill ở lần chơi trước rồi New Game chọn Mercy, cờ Kill cũ vẫn có thể ảnh hưởng boss sau vì BossDialogue kiểm tra GlobalPath_Kill.

**Đề xuất:** một GameSession giữ tiến trình runtime; SaveService tuần tự hóa một snapshot có version. Phân biệt bắt đầu mới, resume và chuyển scene. Có thao tác reset session đầy đủ khi New Game.

**Nguồn:** [SaveSystem.cs](../Assets/_Project/Scripts/UI/SaveSystem.cs#L5), [PlayerHealth.cs](../Assets/_Project/Scripts/Player/PlayerHealth.cs#L199), [MainMenuManager.cs](../Assets/_Project/Scripts/UI/MainMenuManager.cs#L27), [GameProgressManager.cs](../Assets/_Project/Scripts/Core/GameProgressManager.cs#L9), [BossDialogue.cs](../Assets/_Project/Scripts/Core/BossDialogue.cs#L84).

**Kiểm tra:** Save → thoát ứng dụng → Resume phải giữ vị trí, HP và các dữ liệu tiến trình được sản phẩm cam kết lưu; New Game không mang cờ cũ.

### R04 — Cao: Nhánh gây damage bằng dash không có đối tượng nhận

**Bằng chứng:** DashHitbox chỉ gọi OnDashHit khi tìm được IDamageable. Trong 118 script hiện tại, interface được định nghĩa nhưng không có class triển khai. Các lớp EnemyHealth, EnemyBase và BossHealth có các hàm TakeDamage khác nhau, chưa cùng dùng interface đó.

**Đề xuất:** thống nhất hợp đồng nhận damage, kiểu dữ liệu damage và cách tìm receiver ở collider con/parent; bảo đảm mỗi đòn xử lý mục tiêu theo quy tắc được thiết kế.

**Nguồn:** [DashHitbox.cs](../Assets/_Project/Scripts/Player/DashHitbox.cs#L12), [PlayerController.cs](../Assets/_Project/Scripts/Player/PlayerController.cs#L494), [PlayerDamageDealer.cs](../Assets/_Project/Scripts/Player/PlayerDamageDealer.cs#L32).

**Kiểm tra:** dash vào từng loại quái và boss, đo đúng một lần thay đổi HP theo thiết kế.

### R05 — Cao: Tarot có dữ liệu và UI nhưng chưa áp dụng vào gameplay

**Bằng chứng:** PlayerStatManager có các getter multiplier cho HP, speed, damage, cooldown và các chỉ số khác. Tìm trong mã và các persistent event của scene/prefab chưa thấy các getter này được dùng để thay đổi thông số gameplay. PlayerController và PlayerHealth vẫn sử dụng thông số riêng.

Ngoài ra FortuneTellerNPC.Awake xóa PlayerStatIndex khi chạy trong Editor, làm việc kiểm tra persistence khác với bản build.

**Đề xuất:** xác định base stats và effective stats; áp dụng modifier tập trung khi nhận/load/reset Tarot. Chuyển thao tác reset phục vụ debug thành lệnh chủ động.

**Nguồn:** [PlayerStatManager.cs](../Assets/_Project/Scripts/Player/PlayerStatManager.cs#L71), [FortuneTellerNPC.cs](../Assets/_Project/Scripts/AI/NPC/FortuneTellerNPC.cs#L22), [PlayerStatProfile.cs](../Assets/_Project/Scripts/Player/PlayerStatProfile.cs#L45).

**Kiểm tra:** thẻ tăng HP/speed/damage phải làm thay đổi giá trị thực đo được; hiệu ứng không bị cộng lặp khi load scene.

### R06 — Cao: Form biến hình chưa dùng chung HP và cơ chế nhận damage

**Bằng chứng:** trên prefab Player, form Transform_01 có tag Player nhưng chỉ có TransformController/AttackController, không có PlayerHealth hoặc PlayerStats. PlayerTransformManager tách form khỏi parent và tắt collider/Rigidbody của player gốc. Nhiều đòn đối phương tìm Health/Stats ngay trên collider vừa chạm; với form, chúng có thể bỏ qua damage hoặc phát sinh null reference. Các scene có thể có override nên cần kiểm chứng từng scene trong Play Mode.

InventoryToggleUI cũng chỉ bật/tắt PlayerController của player gốc; chưa có điều phối chung cho form hiện tại. Khi đóng inventory trong lúc biến hình, nó có thể bật lại controller gốc ngoài ý định.

**Đề xuất:** giữ một identity/HP/session cho nhân vật; form thay đổi hình ảnh và bộ kỹ năng. Receiver của form chuyển damage về cùng chủ sở hữu HP. Một nơi quản lý input cho form, menu, thoại và trạng thái chết.

**Nguồn:** [Player.prefab](../Assets/_Project/Prefabs/Player/Player.prefab#L365), [PlayerTransformManager.cs](../Assets/_Project/Scripts/Player/PlayerTransformManager.cs#L142), [InventoryToggleUI.cs](../Assets/_Project/Scripts/UI/InventoryToggleUI.cs#L25).

**Kiểm tra:** biến hình → nhận damage → mở/đóng inventory → trở về form thường; HP và số controller hoạt động phải nhất quán.

### R07 — Cao: Inventory có thể xóa item mà không thực hiện đúng hiệu ứng

**Bằng chứng:** InventorySlot gọi UseItem(int). Nhánh này kiểm tra itemName, hồi đầy HP cho tên Potion, chỉ ghi log cho Rune và sau đó luôn đặt slot thành null. Nhánh UseItem(Item) lại xử lý itemType/healAmount theo cách khác. Item khác tên hoặc chưa được triển khai vẫn có thể bị xóa khi double-click.

List giữ các slot null nhưng AddItem chỉ append; thao tác drop vào ô vượt Count bị từ chối. Cần xác định rõ mô hình slot trước khi mở rộng inventory.

**Đề xuất:** chỉ một đường dùng item trả về kết quả thành công/thất bại; chỉ tiêu thụ khi hiệu ứng thành công theo thiết kế. Dùng ID ổn định và định nghĩa item tách khỏi số lượng/trạng thái từng slot.

**Nguồn:** [InventoryManager.cs](../Assets/_Project/Scripts/UI/InventoryManager.cs#L42), [InventorySlot.cs](../Assets/_Project/Scripts/UI/InventorySlot.cs#L236), [InventoryUI.cs](../Assets/_Project/Scripts/UI/InventoryUI.cs#L20).

**Kiểm tra:** dùng Potion theo healAmount, Rune, item chưa hỗ trợ, kéo vào ô trống và nhặt thêm vào các ô đã dùng.

### R08 — Trung bình: Resume chưa bật lại âm thanh

**Bằng chứng:** OpenPause đặt AudioListener.pause = true; ClosePause chỉ đặt lại timeScale. AudioListener.pause không được đặt false trong luồng Resume này. menuQuit cũng đổi scene khi đang pause mà chưa reset các trạng thái toàn cục; Start của MainMenu chưa làm việc đó.

**Đề xuất:** một hàm chuyển trạng thái pause/resume cập nhật cả thời gian, audio, action map và UI focus; trạng thái menu được khởi tạo rõ ràng khi chuyển scene.

**Nguồn:** [PauseMenuManager.cs](../Assets/_Project/Scripts/UI/PauseMenuManager.cs#L95), [MainMenuManager.cs](../Assets/_Project/Scripts/UI/MainMenuManager.cs#L13).

**Kiểm tra:** Pause → Resume; Pause → Main Menu; pause trong khi form đang sử dụng kỹ năng.

### R09 — Trung bình: Graphics settings lưu nhưng chưa được áp khi khởi động

**Bằng chứng:** SettingsManager.Load đọc resolution, quality, fullscreen và vSync rồi chỉ gọi Apply(current), trong khi Apply chỉ xử lý AudioMixer. ApplyGraphics được gọi qua ApplySaved khi người dùng bấm Apply.

**Đề xuất:** áp đầy đủ dữ liệu đã lưu sau khi khởi tạo, kiểm tra giới hạn index và chia sẻ một danh sách resolution giữa manager/UI.

**Nguồn:** [SettingsManager.cs](../Assets/_Project/Scripts/UI/SettingsManager.cs#L42), [GraphicsSettingsPanel.cs](../Assets/_Project/Scripts/UI/GraphicsSettingsPanel.cs#L22).

**Kiểm tra:** đổi settings → lưu → đóng bản build → mở lại, so sánh thông số thực với lựa chọn đã lưu.

### R10 — Trung bình: Save file chưa có đường phục hồi khi lỗi

**Bằng chứng:** ghi trực tiếp vào save.json; đọc/parse không có xử lý lỗi và HasSave chỉ kiểm tra tồn tại. Chưa có schema version hoặc validation giá trị HP/scene.

**Đề xuất:** ghi file tạm rồi thay thế file chính, giữ bản dự phòng phù hợp, validate và migration theo version; thông báo rõ khi save không đọc được.

**Nguồn:** [SaveSystem.cs](../Assets/_Project/Scripts/UI/SaveSystem.cs#L25).

**Kiểm tra:** file rỗng, JSON không hợp lệ, scene đã bỏ và save từ schema cũ; không để Resume ném exception chưa xử lý.

## Nâng cấp sau khi ổn định các luồng trên

1. **Điều khiển và trạng thái:** gom binding về InputActionAsset; có action map Gameplay/UI và quy tắc input cho Dialogue/Paused/Dead/Transforming. Giữ Both trong thời gian chuyển đổi. GameManager đã có enum/event nhưng chưa thấy được kết nối vào các luồng hiện tại.
2. **Tách trách nhiệm:** PlayerController hiện 568 dòng, BossGroundAttack 493 dòng, EnemySpawner 403 dòng. Tách theo movement, combat, abilities và phase khi sửa từng tính năng; dùng config ScriptableObject cho dữ liệu cân bằng. Tránh thay toàn bộ các controller trong cùng một đợt.
3. **Kiểm thử:** chưa có test assembly trong Assets. Thêm EditMode cho tính damage/modifier, item và serialize save; PlayMode cho chết/hồi sinh, biến hình, pause, chuyển scene và New Game/Resume. Chạy thử player build để kiểm tra các nhánh khác Editor.
4. **Hiệu năng:** đo trên Development Build ở map đông quái và các phase boss. Các điểm cần đo là Instantiate/Destroy đạn/VFX, truy vấn Physics2D cấp phát mảng và cập nhật UI. Chọn pooling, cache và atlas dựa trên số đo; hiện chưa có bằng chứng để kết luận FPS hoặc bộ nhớ đang không đạt.
5. **Media/UX:** kiểm tra lại cảnh báo audio/video đã ghi nhận, đồng bộ cutscene, glyph tiếng Việt, focus UI và giao diện ở các tỉ lệ màn hình mục tiêu. Các nhận xét về chất lượng hình/âm thanh cần xem/nghe bản chạy.
6. **Quy trình nhóm:** tạo commit riêng cho đợt di chuyển trước khi fix logic. Git hiện hiển thị các đường dẫn cũ bị xóa và thư mục mới chưa track; khi commit cần đưa cả asset mới và .meta tương ứng vào cùng thay đổi. Chưa có commit nào được tạo trong lượt đánh giá này.

## Thứ tự thực hiện đề xuất

| Đợt | Phạm vi | Điều kiện hoàn thành |
|---|---|---|
| 0 | Chốt thay đổi cấu trúc hiện tại | Checkout sạch vẫn mở được đúng scene/prefab |
| 1 | R01, R02, R04, R06: health, damage, dash, form | Damage nhất quán; hồi sinh và form nhận damage đúng |
| 2 | R03, R05, R07, R10: session/save, Tarot, inventory | New Game/Resume và hiệu ứng item/stat có kiểm thử |
| 3 | R08, R09 và input/state/UI | Pause, âm thanh, settings và UI focus đúng |
| 4 | Player build, profiler, media và polish | Có bản build chạy được và số đo trên máy mục tiêu |

Ưu tiên đi đến một vòng chơi hoàn chỉnh có save/resume, chết/hồi sinh, Tarot và biến hình hoạt động nhất quán, rồi mới mở rộng nội dung dựa trên các hệ thống đó.

## Tài liệu Unity đã đối chiếu

- [AudioListener.pause — Unity 6.3](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioListener-pause.html)
- [StopAllCoroutines — Unity 6.3](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.StopAllCoroutines.html)
- [Input Actions — Input System 1.17](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.17/manual/Actions.html)
- [Profiling an application](https://docs.unity3d.com/2022.2/Documentation/Manual/profiler-profiling-applications.html)
- [Thông báo chính thức về lỗi chữ ký package](https://discussions.unity.com/t/package-manager-error-resolved-new-hub-badge-warning/1706569)

Unity 6000.3.3f1 hiện thuộc nhóm bị lỗi xác thực chữ ký đã được Unity công bố. Người dùng đã chọn hoãn nâng Editor vì dung lượng; lượt đánh giá này giữ nguyên Editor, package, source và scene.
