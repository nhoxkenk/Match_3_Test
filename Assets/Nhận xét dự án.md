Điểm cộng:
- Dự án đã implement một core gameplay match - 3 đầy đủ, khá là mượt mà và logic code đã được seperate nên đã dễ đọc hơn
- luồng Game, UI, Board đã phân biệt được
- Game Manager truyền dependency xuống thay vì để các class tự tìm references, khá là oge
- Luồng coroutine không bị race conditions

Điểm trừ:
- Các trạng thái và tiến trình chưa được wire up một cách thống nhất
- Mặc dù đã thiết kế theo hướng Data driven, nhưng thực sự các class chưa có separation of concern
  	+ Lấy luôn ví dụ về BoardController, nó nên chỉ có quản lý việc quản lý board, handle các input và truyền lại cho board, 
	nó không thật sự cần phải biết về class có quyền higher than chính bản thân nó như GameManager, hay là GameSettings
	+ Architecture của game cần phải định nghĩa rõ vấn đề này, ta nên dung Assembly Definition để định nghĩa rõ hơn, ví dụ như với Module Board và các class phụ trợ của nó như Item, Cell
	thì ta nên đóng gói nó lại thành một asmdef và lúc này ta sẽ quan tâm rang nó nên được tiếp xúc với các Module nào và Module nào nên được tiếp xúc với nó
	trong trường hợp này thì nó nên biết về module UI và được biết bởi module Core gồm có GameManager, vậy thì flow lúc này của ta sẽ là Core => Board => UI
	+ hướng sự thiết kế từ ngoài vào trong và tránh đi sự tiếp xúc 2 chiều giữa các Module, ta sẽ thấy rõ Seperation of Concern hơn, từ đó ta có thể quyết định các Architecture nền móng cho cả
	dự án tùy theo ta muốn đi theo hướng Data-Driven hay là Event-Driven.

