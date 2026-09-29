using System.Collections.Generic;
using System.Threading.Tasks;
using WritingApp.Models;

namespace WritingApp.Services;

public interface IManuscriptService
{
    Task<List<ChapterItem>> LoadChaptersFromFolderAsync(string folderPath);
    Task<ChapterItem> CreateChapterAsync(string folderPath, string chapterTitle);
    Task SaveChapterAsync(ChapterItem chapter);
    int CalculateWordCount(string markdownContent);
}