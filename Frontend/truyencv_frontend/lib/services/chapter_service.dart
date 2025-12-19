import 'dart:convert';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/chapter.dart';
import '../models/api_response.dart';
import 'http_client_helper.dart';

class ChapterService {
  final http.Client _client = HttpClientHelper.createHttpClient();

  // Lấy danh sách chapters theo story ID
  Future<ApiResponse<List<ChapterListItem>>> getChaptersByStory(int storyId) async {
    try {
      final response = await _client.get(
        Uri.parse('${ApiConfig.chaptersEndpoint}/by-story/$storyId'),
        headers: {'Content-Type': 'application/json'},
      );

      final jsonData = json.decode(response.body) as Map<String, dynamic>;
      final apiResponse = ApiResponse.fromJson(jsonData, (data) {
        if (data is List) {
          return data.map((item) => ChapterListItem.fromJson(item as Map<String, dynamic>)).toList();
        }
        return <ChapterListItem>[];
      });

      return apiResponse;
    } catch (e) {
      return ApiResponse(
        status: false,
        message: 'Lỗi kết nối: ${e.toString()}',
        data: null,
      );
    }
  }

  // Lấy chapter theo ID
  Future<ApiResponse<Chapter?>> getChapterById(int id) async {
    try {
      final response = await _client.get(
        Uri.parse('${ApiConfig.chaptersEndpoint}/$id'),
        headers: {'Content-Type': 'application/json'},
      );

      final jsonData = json.decode(response.body) as Map<String, dynamic>;
      final apiResponse = ApiResponse.fromJson(jsonData, (data) {
        if (data != null) {
          return Chapter.fromJson(data as Map<String, dynamic>);
        }
        return null;
      });

      return apiResponse;
    } catch (e) {
      return ApiResponse(
        status: false,
        message: 'Lỗi kết nối: ${e.toString()}',
        data: null,
      );
    }
  }
}

