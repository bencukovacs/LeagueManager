import { useAuth } from '../../contexts/AuthContext';
import type { PlayerResponseDto, Team } from '../../types';
import EditTeamForm from './EditTeamForm';
import RosterManagement from './RosterManagement';

interface TeamManagementProps {
  team: Team;
  roster: PlayerResponseDto[];
  isRosterLoading: boolean;
}

export default function TeamManagement({ team, roster, isRosterLoading }: TeamManagementProps) {
  const { user } = useAuth();

  // Determine if the current user is an admin
  const isAdmin = user?.roles.includes('Admin') ?? false;

  // For now, we assume a Team Leader is viewing their own team page.
  // The authorization for this is handled by the parent page's logic.
  const canManage = true; 

  if (!canManage) {
    return null;
  }

  return (
    <div className="mt-6">
      <h2 className="text-2xl font-semibold border-b pb-2 mb-4">Team Management</h2>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Pass the isAdmin flag down to the roster component */}
        <RosterManagement 
          teamId={team.id} 
          roster={roster} 
          isLoading={isRosterLoading} 
          isAdmin={isAdmin} 
        />
        <EditTeamForm team={team} />
      </div>
    </div>
  );
}