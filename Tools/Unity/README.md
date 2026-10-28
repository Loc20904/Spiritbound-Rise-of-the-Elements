# Công cụ bảo trì hierarchy

`SceneHierarchyMigration.cs` là migration riêng của Spiritbound trên Unity 6000.3.3f1. Nằm ngoài Assets; Unity CLI biên dịch và chạy bằng `script run`. Cần Editor mở đúng project, Pipeline đã tải, và Editor dừng Play Mode. Không nâng Editor/package khi dùng tool này.

```powershell
$project = 'D:\FPT\SP26\Unity_Spirit\Spiritbound-Rise-of-the-Elements'
unity command script run --file "$project/Tools/Unity/SceneHierarchyMigration.cs" --entry SceneHierarchyMigration.Preview --caller plugin --skill unity-cli --project-path $project --format json --timeout 120
unity command script run --file "$project/Tools/Unity/SceneHierarchyMigration.cs" --entry SceneHierarchyMigration.ValidateSaved --caller plugin --skill unity-cli --project-path $project --format json --timeout 180
```

Kiểm tra cả envelope `success`, `data.result.success` và danh sách `errors` của từng scene. `Preview` ghi manifest và đóng preview scene; không save asset. `ValidateSaved` đọc snapshot local, tải từng scene additively rồi đóng scene do tool mở. Scene đã mở với chỉnh sửa chưa lưu sẽ bị từ chối, không Save All.

Migration ngày 28/10 đã chạy trên cả 11 scene. `ApplyPilot`, `ApplyRemaining`, `ApplyPresentation` dành cho checkout chưa migration; đừng chạy lại trên project đã xử lý. `ApplyScene(path)` là hàm dùng chung bên trong tool. Khi có snapshot trước đó tool từ chối Apply để tránh ghi đè mốc rollback. Không xóa snapshot chỉ để vượt kiểm tra này.

Tool tạo container identity và reparent bằng Undo, giữ nội bộ các prefab. Root có service DontDestroyOnLoad vẫn ở root. Không rename, xóa object, unpack prefab hoặc thay component. Phân loại được viết riêng cho các root hiện có; gặp root lạ sẽ dừng để xem xét thay vì tự đoán.

Mỗi scene có `before.unity`, `before.json`, `after.json`, `result.json` trong:

```text
Library/SceneHierarchyMigration/2026-10-28/snapshots/<SceneName>/
```

Thư mục này bị Git ignore và có thể mất khi xóa Library; sao chép ra nơi khác trước khi dọn cache nếu còn cần rollback. Snapshot JSON lưu GlobalObjectId, child order, world matrix, active/tag/layer/static flags, prefab source, component list và serialized values/references. Native reference bookkeeping m_FileID/m_PathID bị bỏ qua; identity thật được đối chiếu bằng GlobalObjectId. Tilemap dùng SHA256 của Editor JSON với reference đã chuẩn hóa. Canvas được cập nhật layout trước khi chụp mốc và sau reparent. Kiểm tra YAML độc lập của đợt migration bổ sung đối chiếu document component và GUID.

MapForest có thêm bản `.json.v1`: snapshot cũ trước tối ưu tilemap. Đã kiểm tra cả 4 Tilemap YAML giống hệt trước khi chuyển định dạng. Không cần chuyển đổi lại.

Trước save, lỗi kiểm chứng tự Undo phần migration của scene đó. Nếu lỗi xảy ra **sau save**, tool dừng và giữ backup để người vận hành khôi phục; không tự ghi đè scene đang mở. Rollback: dừng Play, bảo toàn thay đổi mới, đóng scene liên quan mà không save, khôi phục đúng `before.unity` của scene đó rồi import/mở lại qua Unity. Giữ `.meta` hiện tại; không restore hàng loạt từ HEAD vì IntroScene có sửa đổi từ trước migration. Các scene trước đó trong một batch đã thành công vẫn được giữ.

[Plan và kết quả](../../Docs/SCENE_RESTRUCTURE_PLAN.md), [báo cáo chi tiết](../../Docs/Maintenance/scene-hierarchy-migration-2026-10-28.json). Validation cấu trúc không thay thế lượt chơi hết mọi phase boss/story.
