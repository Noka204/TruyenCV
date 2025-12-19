class ApiConfig {
  // Thay đổi URL này theo địa chỉ backend của bạn
  // Nếu chạy local: http://localhost:5000 hoặc https://localhost:5001
  static const String baseUrl = 'https://localhost:7182';

  // API endpoints
  static const String authorsEndpoint = '$baseUrl/api/authors';
  static const String storiesEndpoint = '$baseUrl/api/stories';
}
