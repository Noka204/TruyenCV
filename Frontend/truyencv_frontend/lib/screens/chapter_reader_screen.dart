import 'package:flutter/material.dart';
import '../models/chapter.dart';
import '../services/chapter_service.dart';
import 'chapters_list_screen.dart';

class ChapterReaderScreen extends StatefulWidget {
  final int chapterId;
  final int storyId;
  final String storyTitle;

  const ChapterReaderScreen({
    super.key,
    required this.chapterId,
    required this.storyId,
    required this.storyTitle,
  });

  @override
  State<ChapterReaderScreen> createState() => _ChapterReaderScreenState();
}

class _ChapterReaderScreenState extends State<ChapterReaderScreen> {
  final ChapterService _chapterService = ChapterService();
  Chapter? _chapter;
  bool _isLoading = true;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _loadChapter();
  }

  Future<void> _loadChapter() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    final response = await _chapterService.getChapterById(widget.chapterId);

    setState(() {
      _isLoading = false;
      if (response.status && response.data != null) {
        _chapter = response.data!;
        _errorMessage = null;
      } else {
        _errorMessage = response.message;
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          _chapter?.title ?? 'Chương ${_chapter?.chapterNumber ?? ""}',
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
        backgroundColor: Colors.purple,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.list),
            onPressed: () {
              Navigator.pushReplacement(
                context,
                MaterialPageRoute(
                  builder: (context) => ChaptersListScreen(
                    storyId: widget.storyId,
                    storyTitle: widget.storyTitle,
                  ),
                ),
              );
            },
            tooltip: 'Danh sách chương',
          ),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _errorMessage != null
              ? Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Icon(Icons.error_outline, size: 64, color: Colors.red),
                      const SizedBox(height: 16),
                      Text(
                        _errorMessage!,
                        style: const TextStyle(color: Colors.red),
                        textAlign: TextAlign.center,
                      ),
                      const SizedBox(height: 16),
                      ElevatedButton(
                        onPressed: _loadChapter,
                        child: const Text('Thử lại'),
                      ),
                    ],
                  ),
                )
              : _chapter == null
                  ? const Center(child: Text('Không tìm thấy chương'))
                  : SingleChildScrollView(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          if (_chapter!.title != null && _chapter!.title!.isNotEmpty)
                            Padding(
                              padding: const EdgeInsets.only(bottom: 16),
                              child: Text(
                                _chapter!.title!,
                                style: const TextStyle(
                                  fontSize: 24,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                          Text(
                            'Chương ${_chapter!.chapterNumber}',
                            style: TextStyle(
                              fontSize: 18,
                              color: Colors.grey.shade600,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                          const SizedBox(height: 24),
                          Text(
                            _chapter!.content,
                            style: const TextStyle(
                              fontSize: 16,
                              height: 1.8,
                              letterSpacing: 0.5,
                            ),
                          ),
                          const SizedBox(height: 32),
                          Divider(color: Colors.grey.shade300),
                          const SizedBox(height: 16),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text(
                                'Lượt đọc: ${_chapter!.readCont}',
                                style: TextStyle(
                                  fontSize: 12,
                                  color: Colors.grey.shade600,
                                ),
                              ),
                              Text(
                                'Cập nhật: ${_formatDate(_chapter!.updatedAt)}',
                                style: TextStyle(
                                  fontSize: 12,
                                  color: Colors.grey.shade600,
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
    );
  }

  String _formatDate(DateTime date) {
    return '${date.day}/${date.month}/${date.year}';
  }
}

