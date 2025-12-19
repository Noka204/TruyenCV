import 'dart:convert';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/story.dart';
import '../models/api_response.dart';
import 'http_client_helper.dart';

class StoryService {
  final http.Client _client = HttpClientHelper.createHttpClient();
  // Lấy tất cả truyện (có thể filter theo authorId, primaryGenreId, q)
  Future<ApiResponse<List<StoryListItem>>> getAllStories({
    int? authorId,
    int? primaryGenreId,
    String? q,
  }) async {
    try {
      final uri = Uri.parse('${ApiConfig.storiesEndpoint}/all').replace(
        queryParameters: {
          if (authorId != null) 'authorId': authorId.toString(),
          if (primaryGenreId != null)
            'primaryGenreId': primaryGenreId.toString(),
          if (q != null && q.isNotEmpty) 'q': q,
        },
      );

      final response = await _client
          .get(
            uri,
            headers: {
              'Content-Type': 'application/json',
              'Accept': 'application/json',
            },
          )
          .timeout(
            const Duration(seconds: 10),
            onTimeout: () {
              throw Exception(
                'Kết nối timeout. Vui lòng kiểm tra backend có đang chạy không.',
              );
            },
          );

      if (response.statusCode == 200) {
        final jsonData = json.decode(response.body) as Map<String, dynamic>;
        
        // Debug: kiểm tra JSON response
        if (jsonData['data'] is List && (jsonData['data'] as List).isNotEmpty) {
          final firstItem = (jsonData['data'] as List)[0] as Map<String, dynamic>;
          print('DEBUG - First story JSON keys: ${firstItem.keys}');
          print('DEBUG - First story coverImage: ${firstItem['coverImage'] ?? firstItem['CoverImage']}');
        }
        
        final apiResponse = ApiResponse.fromJson(jsonData, (data) {
          if (data is List) {
            return data.map((item) {
              final parsed = StoryListItem.fromJson(item as Map<String, dynamic>);
              print('DEBUG - Parsed coverImage: ${parsed.coverImage}');
              return parsed;
            }).toList();
          }
          return <StoryListItem>[];
        });

        return apiResponse;
      } else {
        return ApiResponse(
          status: false,
          message:
              'Lỗi từ server: ${response.statusCode} - ${response.reasonPhrase}',
          data: null,
        );
      }
    } catch (e) {
      String errorMessage = 'Lỗi kết nối: ${e.toString()}';

      if (e.toString().contains('Failed to fetch') ||
          e.toString().contains('NetworkError')) {
        errorMessage =
            'Không thể kết nối đến backend. Vui lòng kiểm tra:\n'
            '1. Backend có đang chạy trên http://localhost:5057 không?\n'
            '2. CORS đã được cấu hình đúng chưa?\n'
            '3. Thử mở http://localhost:5057/api/stories/all trong trình duyệt';
      }

      return ApiResponse(status: false, message: errorMessage, data: null);
    }
  }

  // Lấy truyện theo ID
  Future<ApiResponse<Story?>> getStoryById(int id) async {
    try {
      final response = await _client.get(
        Uri.parse('${ApiConfig.storiesEndpoint}/$id'),
        headers: {'Content-Type': 'application/json'},
      );

      final jsonData = json.decode(response.body) as Map<String, dynamic>;
      final apiResponse = ApiResponse.fromJson(jsonData, (data) {
        if (data != null) {
          return Story.fromJson(data as Map<String, dynamic>);
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

  // Tạo truyện mới
  Future<ApiResponse<Story?>> createStory(StoryCreateDTO dto) async {
    try {
      final response = await _client.post(
        Uri.parse('${ApiConfig.storiesEndpoint}/create'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode(dto.toJson()),
      );

      final jsonData = json.decode(response.body) as Map<String, dynamic>;
      final apiResponse = ApiResponse.fromJson(jsonData, (data) {
        if (data != null) {
          return Story.fromJson(data as Map<String, dynamic>);
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

  // Cập nhật truyện
  Future<ApiResponse<Story?>> updateStory(int id, StoryUpdateDTO dto) async {
    try {
      final response = await _client.put(
        Uri.parse('${ApiConfig.storiesEndpoint}/update-$id'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode(dto.toJson()),
      );

      final jsonData = json.decode(response.body) as Map<String, dynamic>;
      final apiResponse = ApiResponse.fromJson(jsonData, (data) {
        if (data != null) {
          return Story.fromJson(data as Map<String, dynamic>);
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

  // Xóa truyện
  Future<ApiResponse<bool>> deleteStory(int id) async {
    try {
      final response = await _client.delete(
        Uri.parse('${ApiConfig.storiesEndpoint}/delete-$id'),
        headers: {'Content-Type': 'application/json'},
      );

      final jsonData = json.decode(response.body) as Map<String, dynamic>;
      final apiResponse = ApiResponse.fromJson(jsonData, (data) => true);

      return apiResponse;
    } catch (e) {
      return ApiResponse(
        status: false,
        message: 'Lỗi kết nối: ${e.toString()}',
        data: false,
      );
    }
  }
}
