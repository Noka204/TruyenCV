import 'dart:io';
import 'package:flutter/foundation.dart';

class ApiConfig {
  // Tự động chọn URL theo platform
  static String get baseUrl {
    if (kIsWeb) {
      // Chạy trên web: dùng localhost
      return 'http://localhost:5057';
    } else if (Platform.isAndroid) {
      // Chạy trên Android:
      // - Emulator: dùng 10.0.2.2
      // - Thiết bị thật: dùng IP thật của máy tính (ví dụ: 192.168.1.17)
      // Để tìm IP: Windows: ipconfig, Mac/Linux: ifconfig
      return 'http://10.0.2.2:5057'; // Dùng cho emulator
      // return 'http://192.168.1.17:5057'; // IP WiFi của máy tính (dùng cho thiết bị thật)
    } else {
      // Chạy trên iOS/Desktop: dùng localhost
      return 'http://localhost:5057';
    }
  }

  // API endpoints
  static String get authorsEndpoint => '$baseUrl/api/authors';
  static String get storiesEndpoint => '$baseUrl/api/stories';
}
