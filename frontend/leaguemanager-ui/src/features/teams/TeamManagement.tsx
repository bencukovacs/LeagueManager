import { useAuth } from '../../contexts/AuthContext';
import type { PlayerResponseDto, Team } from '../../types'; 
import EditTeamForm from './EditTeamForm';
import RosterManagement from './RosterManagement';

// 2. Update the props to include the roster and its loading state
interface TeamManagementProps {
  readonly team: Team;
  readonly roster: PlayerResponseDto[];
  readonly isRosterLoading: boolean;
}

export default function TeamManagement({ team, roster, isRosterLoading }: TeamManagementProps) {
  const { user } = useAuth();

  const canManage = user?.roles.includes('Admin');

  if (!canManage) {
    return null;
  }

  return (
    <div className="mt-6">
      <h2 className="text-2xl font-semibold border-b pb-2 mb-4">Team Management</h2>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Pass the props down to the RosterManagement component */}
        <RosterManagement teamId={team.id} roster={roster} isLoading={isRosterLoading} />
        <EditTeamForm team={team} />
      </div>
    </div>
  );
}