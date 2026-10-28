# Kế hoạch tái cấu trúc hierarchy scene

Ngày đánh giá: **27/10/2026**. Trạng thái ngày **28/10/2026**: **đã triển khai pass gom nhóm cả 11 scene**. AppRoot/SceneContext và hợp nhất prefab chưa triển khai; xem mục 8 để phân biệt kết quả thực tế với lộ trình bên dưới.

Phạm vi là GameObject/hierarchy trong 11 scene build và những prefab dùng chung có liên quan. Đây là đợt riêng với lần di chuyển thư mục Assets đã commit. Giữ Unity 6000.3.3f1 và tên scene hiện tại để tương thích save/chuyển màn.

## 1. Hiện trạng đã kiểm tra

Đọc các scene bằng `EditorSceneManager.OpenPreviewScene`, đóng preview sau mỗi scene. Scene người dùng đang mở vẫn là MapVolcano và giữ trạng thái không dirty trước/sau. Không chạy Play Mode, không save scene/prefab trong lượt đánh giá này.

Tổng cộng: **1.448 GameObject, 281 root object, 15 PlayableDirector**. Không phát hiện Missing Script. Kiểm tra tĩnh các persistent event đang bật không phát hiện target/hàm bị mất; kết quả này không thay thế kiểm thử click/phím khi chạy.

| Scene | Root | Tổng object | Canvas | EventSystem tổng / đang hoạt động |
|---|---:|---:|---:|---:|
| IntroScene | 8 | 18 | 1 | 0 / 0 |
| FortuneScene | 20 | 73 | 3 | 1 / 1 |
| MapVolcano | 75 | 221 | 3 | 1 / 1 |
| BossFireScene | 28 | 195 | 6 | 2 / 1 |
| MapForest | 19 | 147 | 3 | 1 / 1 |
| BossGroundScene | 40 | 192 | 4 | 1 / 1 |
| CreditsScene | 4 | 17 | 1 | 1 / 1 |
| MainMenuScene | 5 | 11 | 1 | 1 / 1 |
| MapIce | 23 | 154 | 3 | 1 / 1 |
| BossWaterScene | 27 | 220 | 4 | 1 / 1 |
| FinalBossScene | 32 | 200 | 6 | 2 / 1 |

Từng root, prefab nguồn, service, camera, canvas và object test được ghi trong [báo cáo kiểm kê](Maintenance/scene-hierarchy-audit-2026-10-27.json). Báo cáo là dữ liệu kiểm kê; trước migration vẫn cần snapshot đầy đủ transform, binding và tham chiếu để rollback.

## 2. Vấn đề và giải pháp

### H01 — Quá nhiều object cùng cấp, thiếu ranh giới chức năng

MapVolcano có 75 root, gồm hàng loạt Lava, Spike, spikeAbove, FallTrap, platform, checkpoint và UI. BossGroundScene có 40 root, trong đó platforms_1 và các bản sao cùng nằm cạnh Timeline, boss, camera và UI.

**Giải pháp:** nhóm theo World / Actors / Cameras / UI / Audio / Sequences / Encounter, chia World tiếp thành Terrain, Backgrounds, Platforms, Hazards và Checkpoints/Transitions khi có nội dung. Nhóm chỉ là container có transform identity, không gắn collider/Rigidbody/logic gameplay.

MapVolcano đã có **69 root là prefab instance**. Giữ các instance đó và gom nhóm; không dựng lại toàn bộ scene hoặc unpack prefab để sắp xếp.

### H02 — Tên object khó tìm và không thống nhất

Có GameObject, Grid trùng tên ở FortuneScene; Canvas trùng tên ở FinalBossScene; CameraLimit / CameralLimit / LimitCamera; AudioManger, SFXPoll, Bounary, FrameName/framename và tên sprite nhập vào dùng làm tên gameplay.

**Giải pháp:** dùng tên thể hiện vai trò ở các root/group mới; đổi tên instance sau khi có bản đồ tham chiếu. Ví dụ CameraBounds, SceneMusic, BossTitleCanvas, IntroTimeline. Phân biệt tên prefab tài nguyên và tên instance trong scene.

Không tự đổi tên các node được truy cập bằng chuỗi. Đặc biệt `BossSkillsHolder`, `Btn_Apply`, `Btn_Reset`, `Btn_Back` phải giữ đến khi đã chuyển code sang reference rõ ràng.

### H03 — Ownership và vòng đời service chưa thống nhất

SceneController, SettingsManager, PlayerStatManager, GameProgressManager và SFXPool gọi DontDestroyOnLoad. SceneController có mặt trong Intro, Fortune, ba map thường và BossFire, nhưng không có trong BossGround/BossWater/FinalBoss ở trạng thái asset hiện tại. PlayerStatManager nằm ở Fortune; SFXPool được đặt riêng trong bốn scene boss. Việc mở trực tiếp scene và đi theo luồng game vì vậy có đường khởi tạo khác nhau; cần kiểm thử chứ chưa kết luận mọi đường đều lỗi.

**Giải pháp hai bước:**

1. Trong đợt gom nhóm đầu, giữ nguyên các service persistent ở root. `_Systems` chỉ nhận đối tượng thuộc scene.
2. Sau pilot, tạo AppRoot do AppBootstrap quản lý, chỉ AppRoot là root persistent. Các service được khởi tạo một lần, tái sử dụng khi chuyển scene; SceneContext giữ reference của scene như player, camera, UI, spawn và encounter. Chuyển các lời gọi DontDestroyOnLoad của component con sang ownership ở AppRoot trước khi đặt chúng làm con.

MainMenuScene vẫn là scene đầu của bản build theo yêu cầu trước. Không đưa một scene Bootstrap mới lên trước Main Menu. Khởi tạo service phải hỗ trợ cả luồng MainMenu → Intro → Fortune và Play trực tiếp một map/boss để debug.

Unity chỉ hỗ trợ DontDestroyOnLoad trên root GameObject/component ở root; gom các service hiện tại vào một empty object mà giữ nguyên code sẽ làm thay đổi vòng đời. [Unity API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Object.DontDestroyOnLoad.html).

### H04 — UI dùng chung nhưng cách lắp ghép còn rời rạc

Bảy scene gameplay đều có GameUIComponent, Inventory_Canvas, HPBar_Canvas và SettingsManager đặt riêng. EventSystem nằm trong GameUIComponent; Fortune lại đặt EventSystem dưới Canvas. BossFire và FinalBoss còn một EventSystem ngoài root đang tắt.

**Giải pháp:** lấy GameUIComponent hiện có làm nền cho prefab GameplayUI, tái sử dụng các prefab HUD/inventory/settings dạng nested prefab. Một owner quản lý input UI; mỗi scene có UI tương tác chỉ có một EventSystem hoạt động. SceneContext bind player và các reference phụ thuộc scene; không lưu reference sang scene khác trong prefab asset.

Giữ Canvas riêng cho HUD, pause/settings, dialogue/choice và cutscene khi chúng có sorting/lifecycle khác nhau. Không gộp tất cả thành một Canvas và không làm phẳng Dropdown/ScrollView để giảm độ sâu. Độ sâu tối đa 11 hiện nằm ở cấu trúc Template/Viewport/Content của dropdown.

Chỉ bỏ EventSystem đang tắt sau khi đã kiểm tra reference và Timeline/activation. Hai EventSystem được lưu trong scene không có nghĩa hiện tại cả hai cùng chạy hoặc đã được chứng minh là nguyên nhân lỗi menu.

### H05 — Cutscene, boss phase, anchor và object đang tắt bị trộn

BossWater có Dragon đang tắt với 18 con; FinalBoss có nhiều FinalBoss_Ulti_0, title/cutscene Canvas và VFX đang tắt. Các scene boss có tổng cộng 13 PlayableDirector, cộng Intro/Fortune thành 15.

**Giải pháp:** có EncounterRoot riêng với Boss, PhaseControllers, Anchors và các tham chiếu Timeline/FX; giữ anchors ở sibling của boss đang di chuyển, không đặt điểm teleport/spawn cố định dưới transform của boss. Cinematic presentation được nhóm vào Sequences theo từng đoạn.

Object inactive không được coi là rác. Trước xóa phải kiểm tra reference trong component, UnityEvent, Timeline, animation và code tạo/bật object khi chạy, rồi thử hết phase liên quan. Không suy ra object vô dụng chỉ từ tên hoặc activeSelf=false.

### H06 — Có phụ thuộc cấu trúc và nội dung debug trong prefab production

ParallaxController lấy các layer theo `GetChild(i)`; PauseMenuManager tìm các nút bằng `transform.Find`; PlayerTransformManager tách fire form khỏi parent lúc chạy. ItemTester vẫn gắn trên Inventory_Canvas ở bảy scene gameplay, dù việc đổ đồ được chặn bởi populateOnStart.

**Giải pháp:** giữ nguyên cấu trúc nội bộ và sibling order của player, parallax, UI và prefab boss trong pass đầu. Sau đó thay lookup bằng field/binding phù hợp, đưa script/demo setup sang test rig riêng trong Sandbox. Không chuyển cả Inventory_Canvas sang nhóm Debug chỉ vì trên đó có ItemTester.

## 3. Hierarchy đích

Các nhóm sau chỉ tạo khi có nội dung. Mục tiêu map thường có khoảng 6–9 nhóm/root tổ chức chính sau khi ownership service ổn định; đây là mục tiêu dễ tìm, không phải quota phải đạt bằng mọi giá.

```text
AppRoot                      [global, tạo/tái sử dụng ở runtime]
_Systems                     [SceneContext, logic riêng của scene]
_Cameras
  Main Camera
  CameraRig
  CameraBounds
_World
  Terrain
  Backgrounds
  Platforms
  Hazards
  Checkpoints
  Transitions
_Actors
  Player                     [giữ nguyên nội bộ prefab]
  Enemies
  NPCs
_Encounter                   [chỉ scene boss]
  Boss                       [giữ nguyên nội bộ prefab]
  PhaseControllers
  Anchors
_UI
  GameplayUI                 [từ GameUIComponent hiện có]
    EventSystem
    HUD
    Inventory
    PauseAndSettings
  DialogueAndChoices
_Sequences                   [Timeline, title, cinematic presentation]
_Audio                       [nhạc/ambient thuộc scene]
_Runtime                     [chỉ tạo khi chạy: projectile, VFX, form tách rời]
```

MainMenu/Intro/Credits dùng mẫu gọn: Cameras, UI, Audio và Systems/Sequences nếu cần. Không thêm Actors/World/Encounter rỗng vào các scene này.

## 4. Pilot MapForest

Chọn MapForest trước vì có đầy đủ player, quái, parallax, UI và chuyển scene, nhưng chỉ có 19 root; dễ kiểm chứng hơn bắt đầu ở map 75 root hoặc boss nhiều phase.

| Object hiện tại | Nhóm đích trong pilot |
|---|---|
| Main Camera, CinemachineCamera, CameralLimit | _Cameras |
| Grid | _World/Terrain |
| BackGround1, Background2 | _World/Backgrounds; giữ nguyên các con |
| Platform_0, Platform_0 (1) | _World/Platforms |
| FinishPoint | _World/Transitions |
| Player | _Actors |
| BotEarth_QDX, botEarth-QCC, BotEarth-Fly | _Actors/Enemies |
| HPBar_Canvas, Inventory_Canvas, GameUIComponent | _UI; giữ nguyên nội bộ |
| AudioManger | _Audio |
| Game Manager, SettingsManager | Giữ nguyên root trong pilot vì đang persistent |

Pilot có thể giảm từ 19 xuống khoảng **7 root**: 5 nhóm và 2 service cũ. Không đổi tên instance, đổi component, xóa object hoặc thay logic trong commit pilot. Kiểm tra parallax và UI sau khi reparent, không chỉ nhìn hierarchy.

## 5. Thứ tự triển khai

| Đợt | Phạm vi | Điều kiện hoàn thành |
|---|---|---|
| 0. Chốt baseline và lỗi đầu vào | Chụp diff hiện có; tách sửa menu đang dang dở; hoàn tất MainMenuScene ở index 0 và kiểm tra click/phím; lưu snapshot scene/reference | Có mốc rollback và phân biệt được lỗi trước/sau migration |
| 1. Quy ước, công cụ và pilot | Manifest phân loại từng root; tool có Preview/Apply/Undo; áp dụng MapForest theo bảng trên | World pose, active state, prefab link, reference, parallax và UI không đổi |
| 2. Ownership và prefab dùng chung | AppBootstrap/AppRoot, SceneContext, GameplayUI, CameraRig; xử lý EventSystem thừa sau kiểm tra | Play trực tiếp scene và chuyển scene đều đủ service; không tích lũy singleton/EventSystem/AudioListener |
| 3. Ba map thường | MapVolcano, MapIce; đồng bộ lại MapForest với prefab/context mới | Trap, platform, checkpoint, camera, inventory và finish point hoạt động |
| 4. Bốn scene boss | BossFire → BossGround → BossWater → FinalBoss; xử lý từng scene riêng | Intro, mọi phase, teleport/spawn, chết, Kill/Mercy và chuyển scene đúng |
| 5. Scene trình bày | Fortune, Intro, Credits, MainMenu | Tarot, dialogue, Timeline/video, menu và luồng game không mất binding |
| 6. Dọn nội dung và bàn giao | Kiểm tra object debug/không dùng, xóa theo danh sách đã chứng minh; validator, tài liệu, Development Build | Không Missing Script/reference mới; hồi quy đạt, hierarchy nhất quán và có báo cáo build |

Không chuyển cả 11 scene trong một commit. Pilot và thay ownership là hai commit khác nhau; mỗi boss có commit riêng để rollback độc lập. Đợt 0/1 được triển khai trước, chỉ mở rộng sau khi pilot đạt kiểm thử.

## 6. Cách migration và kiểm thử

- Snapshot trước thay đổi: scene GUID/hash, GlobalObjectId/fileID của object, parent/sibling index, world transform, tag/layer/static flag/activeSelf, prefab source và overrides, serialized references, UnityEvent và Timeline bindings.
- Manifest theo object ID và prefab nguồn; không chỉ dựa vào tên, vì Fortune có hai Grid và FinalBoss có hai Canvas.
- Tạo container tại position 0, rotation identity, scale 1; di chuyển qua Unity API có Undo. `SetParent(..., true)` giữ world pose; vẫn phải kiểm chứng riêng RectTransform, parallax, collider và animation. [Unity API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Transform.SetParent.html).
- Không unpack prefab, reset transform/layer, thay static flag hoặc sửa thứ tự con của đối tượng nhạy cảm để làm đẹp hierarchy. Rename và thay lookup được làm sau, với mapping và test riêng.
- Không gọi Save All lên scene/asset đang dirty của người dùng. Chỉ save scene/prefab thuộc đợt đó; khi có thay đổi chưa lưu cần checkpoint rõ ràng trước migration.
- Sau mỗi scene: đối chiếu snapshot, không mất object gốc ngoài danh sách xóa đã duyệt; prefab/reference/UnityEvent/Timeline vẫn hợp lệ. Nhóm mới được phép tăng số object nên không dùng tổng object không đổi làm tiêu chí duy nhất.
- PlayMode: di chuyển, nhảy, dash, đánh nhiều đòn, biến hình, damage/chết/hồi sinh; pickup/use inventory; pause/settings và chuột/phím UI; camera bounds/parallax; checkpoint và finish point. Boss phải chạy hết các phase và lựa chọn story, không chỉ vào Play rồi thoát.
- Regression hiện có: 12 EditMode + 31 PlayMode ở đợt rework trước. Chạy lại sau thay đổi có ảnh hưởng và thêm test cho ownership, direct-scene entry, reference/binding bị mất. Các số này là kết quả trước migration, không phải chứng nhận hierarchy mới đã đạt.
- Cuối đợt: mở cả 11 scene, chạy luồng MainMenu → New Game → Intro → Fortune → map/boss, Save/Resume và Credits/Menu; tạo Windows Development Build. Không dùng số root thấp để khẳng định FPS tốt hơn khi chưa đo profiler.

## 7. Ranh giới phạm vi

Lượt lập kế hoạch chỉ tạo tài liệu và báo cáo kiểm kê. Chưa gom/xóa/đổi tên object, chưa đổi thứ tự build scene và chưa sửa menu. Những thay đổi đang có ở font, package, settings, IntroScene và MainMenu_Canvas phải được giữ riêng, không reset hoặc nhập lẫn vào migration.

Các điểm cần quyết định khi tới đợt tương ứng: object inactive nào thực sự không dùng, service audio nào global/scene-local, bộ HUD nào cần giữ khác nhau và chính sách checkpoint world. Không chặn pilot gom nhóm MapForest vì các quyết định nội dung này.

## 8. Kết quả triển khai ngày 28/10/2026

Đã hoàn thành pass tổ chức hierarchy cho cả 11 scene bằng Unity Editor API. Phần đánh giá, thứ tự đợt và các rủi ro ở trên là baseline/lộ trình; kết quả dưới đây chỉ xác nhận pass gom nhóm, không đánh dấu toàn bộ ownership/prefab/cleanup là hoàn tất.

| Scene | Root trước → sau | Container mới |
|---|---:|---:|
| BossFireScene | 28 → 12 | 19 |
| BossGroundScene | 40 → 10 | 21 |
| BossWaterScene | 27 → 10 | 20 |
| CreditsScene | 4 → 3 | 3 |
| FinalBossScene | 32 → 10 | 18 |
| FortuneScene | 20 → 10 | 14 |
| IntroScene | 8 → 4 | 7 |
| MainMenuScene | 5 → 3 | 4 |
| MapForest | 19 → 7 | 12 |
| MapIce | 23 → 7 | 14 |
| MapVolcano | 75 → 7 | 14 |

Tổng root **281 → 83**. Giữ đủ **1.448 object gốc**, thêm **146 container** (tổng 1.594). Không xóa/đổi tên object gốc, không unpack prefab hoặc thay logic gameplay. Persistent service vẫn ở root để giữ DontDestroyOnLoad; mỗi scene chỉ có nhóm có nội dung. Nội bộ player/parallax/boss/UI prefab giữ nguyên. MainMenuScene đã lưu tại build index 0.

Mỗi scene có snapshot trước/sau và mapping GlobalObjectId. Kiểm tra ngay sau di chuyển, sau save và sau tải lại đều không có sai lệch được báo cáo. Kiểm tra YAML không mất document gốc và không đổi document component ngoài Transform/RectTransform, PrefabInstance và SceneRoots. Không có Missing Script/UnityEvent target-method bị mất; giữ đủ 15 PlayableDirector và dữ liệu binding.

Credits từng bị chặn do Layout Group tự cập nhật khi ForceUpdateCanvases, tái hiện được trên scene chưa reparent. Lần thử đó đã Undo và file giữ nguyên byte; lấy baseline sau cập nhật layout rồi chạy lại đã đạt. Không thay cấu hình layout để né validator.

Kiểm chứng thực tế: **12/12 EditMode + 31/31 PlayMode** đạt. Smoke MapForest có frame tiến triển, di chuyển, pause/resume, biến hình/hoàn nguyên, shared health owner, camera/parallax và inventory owner đạt; Console không có error trong lượt này. MainMenu nhận click chuột, điều hướng và Enter; callback runtime được thay bằng spy để không xóa save/quit. Persistent callback thật được kiểm tra target/method riêng.

Windows Development Build **Succeeded**, 0 error, 14 warning lặp về hai transition Transform_01 thiếu điều kiện/Exit Time. Bản build khoảng 722 MiB, tái sử dụng `%TEMP%/spiritbound-rework/Build/Spiritbound.exe`.

Công cụ và hướng dẫn: [Tools/Unity/README.md](../Tools/Unity/README.md). Kết quả chi tiết: [scene-hierarchy-migration-2026-10-28.json](Maintenance/scene-hierarchy-migration-2026-10-28.json). Snapshot/backup scene nằm ngoài Git tại `Library/SceneHierarchyMigration/2026-10-28`; giữ thư mục này trước khi xóa Library nếu cần rollback các chỉnh sửa chưa commit.

Các đợt **AppRoot/SceneContext**, prefab **GameplayUI/CameraRig**, `_Runtime`, rename, bỏ ItemTester/EventSystem inactive và cleanup nội dung vẫn còn trong lộ trình. Cần lượt chơi hết phase boss và luồng story/checkpoint/inventory để nghiệm thu toàn game. Chưa commit trong lượt triển khai này; các chỉnh sửa có sẵn ở Intro, menu, font, package và settings được giữ riêng theo baseline.
