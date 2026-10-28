# Cấu trúc dự án

## Cây thư mục

```text
Assets/
├── _Project/
│   ├── Art/
│   │   ├── Animations/       # Animator, AnimationClip, Timeline, signal
│   │   ├── Fonts/            # Font riêng của game
│   │   ├── Materials/        # Material của môi trường và nhân vật
│   │   ├── Sprites/          # Nhân vật, boss, UI, vật phẩm, projectile
│   │   └── Tilemaps/         # Tile, nền và palette
│   ├── Audio/                # Nhạc, SFX; mixer ở Audio/Mixers
│   ├── Data/                 # ScriptableObject, hội thoại và dữ liệu chỉ số
│   ├── Input/                # InputActionAsset
│   ├── Physics/Materials/    # PhysicsMaterial2D
│   ├── Prefabs/              # AI, nhân vật, UI, world, projectile, variant
│   ├── Scenes/               # 11 scene đang bật trong Build Settings
│   ├── Scripts/              # Toàn bộ 118 script game, theo trách nhiệm
│   ├── Settings/Rendering/   # URP, Renderer2D, volume và global settings
│   └── Video/                # Cutscene và video UI
├── _Sandbox/
│   ├── Recovery/             # 8 bản khôi phục scene cũ, giữ lại để tham khảo
│   ├── Scenes/               # Scene thử nghiệm, không nằm trong Build Settings
│   └── SceneTemplates/       # Template tạo scene và scene phụ thuộc
└── TextMesh Pro/             # Tài nguyên vendor; giữ Resources và shader cần dùng
```

`Docs/` chứa tài liệu của dự án và bản ghi bảo trì. `Packages/` và `ProjectSettings/` vẫn nằm ở thư mục gốc theo cấu trúc Unity.

## Đặt nội dung mới

- Chọn thư mục theo loại tài nguyên trước, rồi theo tính năng như `Player`, `AI`, `UI`, `World` hoặc nguyên tố.
- Đặt C# trong `_Project/Scripts`; đặt prefab, font, video và vật liệu ở các nhóm tương ứng.
- Script thử nghiệm hỗ trợ gameplay hiện có nằm ở `Scripts/Debug`. Đợt sắp xếp này giữ nguyên cách các script được biên dịch.
- Scene dùng để thử nghiệm thuộc `_Sandbox/Scenes`. Chỉ thêm scene vào Build Settings khi đã quyết định đưa vào luồng game.
- Giữ nguyên tên scene hiện có: mã lưu game và chuyển scene đang sử dụng tên dưới dạng chuỗi.
- Khi cần thêm `Resources`, `Editor`, `Plugins` hoặc `StreamingAssets`, giữ đúng tên và ý nghĩa đặc biệt mà Unity quy định.

## Di chuyển và Git

- Di chuyển hoặc đổi tên asset trong Unity Project window hoặc qua `AssetDatabase.MoveAsset`.
- Luôn đưa asset và `.meta` tương ứng vào cùng thay đổi Git. GUID liên kết script, prefab, sprite, animation và scene.
- Không tạo lại các thư mục `Prefab`, `Prelabs`, `Sprite`, `Sound` ở gốc `Assets`; dùng cấu trúc ở trên.
- Không đưa `Library`, `Temp`, `Logs`, `UserSettings`, project/solution IDE hoặc `UIElementsSchema` sinh tự động vào Git. Các quy tắc đã nằm trong `.gitignore`.
- Tài nguyên `TextMesh Pro` giữ vị trí mặc định để các công cụ import của TMP tiếp tục tìm được nội dung. Demo `Examples & Extras` đã được loại sau khi kiểm tra cả GUID và phụ thuộc kiểu C#.

## Đợt tái cấu trúc ngày 24/10/2026

- Di chuyển 2.506 file asset bằng AssetDatabase, giữ GUID; gom các prefab, sprite và script từng nằm rải rác.
- Xóa 136 file demo TMP cùng 8 `.gitkeep` trống, các `.meta` đi kèm và thư mục trống không được tham chiếu.
- Loại `FETCH_HEAD` ở gốc repo, solution `.slnx` và bộ schema UI Toolkit sinh tự động khỏi cây nguồn.
- Giữ cả 8 scene recovery và các scene thử nghiệm. Chưa dùng không đồng nghĩa với file rác.
- Cập nhật đường dẫn của 11 scene build qua GUID, giữ nguyên tên, thứ tự và cờ bật/tắt.
- Kiểm tra trước/sau: cả 11 scene mở được, số root object không đổi và không có Missing Script; 118 script giữ nguyên byte; 112 prefab tải được và không có Missing Script; không xuất hiện GUID bị mất.
- Unity tạm tái tạo atlas font động khi mở scene kiểm tra. Sau khi đóng Editor nền, checksum của toàn bộ 2.547 file asset được giữ lại khớp bản đầu vào, bao gồm cả các chỉnh sửa có sẵn trước đợt này.
- Phạm vi kiểm tra là import, cấu trúc, tham chiếu và khả năng mở scene; chưa kiểm thử lại toàn bộ gameplay hay tạo player build.

Nhật ký thao tác và kết quả kiểm tra: [restructure-2026-10-24.json](Maintenance/restructure-2026-10-24.json).


## Hierarchy scene — 28/10/2026

11 scene build đã được gom theo `_Systems`, `_Cameras`, `_World`, `_Actors`, `_Encounter`, `_UI`, `_Sequences`, `_Audio`; chỉ tạo nhóm có nội dung. Persistent service cũ giữ root. Không đổi tên scene/object hoặc nội bộ prefab để bảo toàn lookup và animation binding. MainMenuScene ở build index 0.

Object mới nên vào nhóm đúng trách nhiệm; container có position 0, rotation identity, scale 1 và không có logic/collider/Rigidbody. Giữ anchor cố định dưới Encounter/Anchors, tách khỏi boss di chuyển. Chi tiết kết quả và các phần ownership/prefab còn lại: [SCENE_RESTRUCTURE_PLAN.md](SCENE_RESTRUCTURE_PLAN.md).

`Tools/Unity/` chứa công cụ bảo trì chạy qua Unity CLI; đặt ngoài Assets để không đưa vào runtime build. Snapshot migration nằm trong Library và không commit.
