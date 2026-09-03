namespace Template.MobileApp.Services;

using Template.MobileApp.Models.Sample.Calendar;

// スケジュールイベントの供給元の抽象 (B-9)。サンプル実装は ScheduleService
public interface IScheduleEventProvider
{
    IReadOnlyList<ScheduleEvent> GetEvents(DateOnly startDate, DateOnly endDate);

    IReadOnlyList<Stamp> GetStamps(DateOnly startDate, DateOnly endDate);
}
