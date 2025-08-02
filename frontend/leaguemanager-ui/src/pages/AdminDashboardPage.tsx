import PendingTeamsList from '../features/admin/PendingTeamsList';
import PendingResultsList from '../features/admin/PendingResultsList';

export default function AdminDashboardPage() {
  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold mb-6">Admin Dashboard</h1>
      <div className="space-y-8">
        <PendingTeamsList />
        <PendingResultsList />
      </div>
    </div>
  );
}