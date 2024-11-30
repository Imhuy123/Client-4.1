- Giới thiệu: 
  + Đây là ứng dụng code nhắn tin sử dụng socket để nhắn tin thông qua một sever trung gian,Sử dụng thư viện để mã hóa Bouncy Castle 
- Điều kiện:
   + Chỉ khi Server được bật  thì mới có thể sử dụng và kết nối thành công được 
   +  Có kết nối mạng, sử dụng dotnet8
- Hướng dẫn sử dụng
  + Ở ô nhập ip kết nối thay thế bằng tên miền mà sever đc trỏ ( mặc định là huynas.synology.com )thành địa chỉ ip puplic nơi mà mà router đc kết nối với sever
  + Giữ nguyên port kết nối là 8081 
- Chức năng:
  + THực hiện kết nối đến Server 
  + Hiển thị danh sách các người kết nối nhận được từ sever 
  + Chọn từ những người kết nối trong danh sách sẽ tự động chuyển qua danh sách nhắn tin 
  + Double click vào danh sách nhắn tin để mở Chatfom để thực hiện nhắn tin
  + Khi tin nhắn đến nếu như chưa mở cửa sổ chat sẽ tự động mở cửa sổ chát lên mà ko cần double click (nếu đã cửa sổ đã mở rồi thì sẽ không mở lên nữa tránh spam) 
  + Có thể nhắn cùng 1 lúc với nhiều người không giới hạn kết nối 
  + Nội dung tin nhắn được mã hóa 
