import PendingTeamsList from '../features/admin/PendingTeamsList';
import PendingResultsList from '../features/admin/PendingResultsList';

export default function AdminDashboardPage() {
  return (
    // The main title is now in the layout, so we just need the content
    <div className="space-y-8">
      <PendingTeamsList />
      <PendingResultsList />
    </div>
  );
}