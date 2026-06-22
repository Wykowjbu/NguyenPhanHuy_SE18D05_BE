 ### 1.  $filter  (Lọc dữ liệu)
  Sử dụng để tìm kiếm hoặc lọc các bản ghi theo điều kiện.
  • Lọc bài viết thuộc danh mục số 1:
  │  GET /api/NewsArticle/public?$filter=CategoryId eq 1

  • Lọc bài viết có tiêu đề chứa chữ "hot":
  │  GET /api/NewsArticle/public?$filter=contains(NewsTitle, 'hot')

  • Điều kiện kết hợp (AND/OR):
  │  GET /api/NewsArticle/public?$filter=CategoryId eq 1 and contains(NewsTitle, 'hot')
  ### 2.  $expand  (Lấy kèm dữ liệu của bảng liên kết - JOIN)

  Vì bạn trả về Entity gốc  IQueryable<NewsArticle> , bạn có thể tự động lấy luôn thông tin chi tiết của  Category  và  Tags  thay vì chỉ lấy ID.

  • Lấy danh sách bài báo KÈM THEO thông tin của Danh mục (Category):
  │  GET /api/NewsArticle/public?$expand=Category

  • Lấy danh sách bài báo kèm theo cả Danh mục và danh sách Tags của bài đó:
  │  GET /api/NewsArticle/public?$expand=Category,Tags

  ### 3.  $select  (Chỉ lấy một số cột nhất định)

  Giúp tối ưu băng thông bằng cách không lấy toàn bộ các trường trong bảng.

  • Chỉ lấy  NewsTitle ,  Headline  và  CreatedDate :
  │  GET /api/NewsArticle/public?$select=NewsTitle,Headline,CreatedDate

  • Kết hợp  select và expand  (Lấy tiêu đề bài báo và chỉ lấy tên của danh mục):
  │  GET /api/NewsArticle/public?select = NewsTitleexpand=Category($select=CategoryName)

  ### 4.  $orderby  (Sắp xếp dữ liệu)

  • Sắp xếp bài báo mới nhất lên đầu (Giảm dần):
  │  GET /api/NewsArticle/public?$orderby=CreatedDate desc
  • Sắp xếp theo CategoryId tăng dần, sau đó theo Ngày tạo giảm dần:
  │  GET /api/NewsArticle/public?$orderby=CategoryId asc, CreatedDate desc

  ### 5.  top và skip  (Phân trang - Pagination)
  • Chỉ lấy 5 bài viết đầu tiên:
  │  GET /api/NewsArticle/public?$top=5

  • Bỏ qua 10 bài viết đầu, lấy 5 bài tiếp theo (tương đương với việc đang ở Trang 3, mỗi trang 5 bài):
  │  GET /api/NewsArticle/public?skip = 10top=5


  ### 6.  $count  (Đếm tổng số dòng)
  Rất hữu ích khi làm UI phân trang để biết có tổng cộng bao nhiêu records. Nó sẽ thêm trường  @odata.count  vào JSON trả về.
  • Đếm tổng số bài viết public:
  │  GET /api/NewsArticle/public?$count=true

  • Vừa phân trang vừa trả về tổng số để vẽ UI (Lấy 5 bài nhưng vẫn đếm tổng database):
  │  GET /api/NewsArticle/public?top = 5count=true