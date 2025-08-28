import { useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';

interface LeaveTeamButtonProps {
    teamStatus: 'PendingApproval' | 'Approved' | 'Rejected';
    userRole: 'Leader' | 'AssistantLeader' | 'Member'; // 1. Add the new prop
}

const leaveMyTeam = async () => {
    await apiClient.delete('/my-team');
};

export default function LeaveTeamButton({ teamStatus, userRole }: LeaveTeamButtonProps) {
    const queryClient = useQueryClient();

    const mutation = useMutation({
        mutationFn: leaveMyTeam,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['myTeamAndConfig'] });
            queryClient.invalidateQueries({ queryKey: ['myTeamStatus'] });
        },
    });

    // 2. The logic now checks both the team status and the user's role.
    const isLeaderOfPendingTeam = userRole === 'Leader' && teamStatus === 'PendingApproval';
    const buttonText = isLeaderOfPendingTeam ? 'Cancel Application' : 'Leave Team';
    const confirmMessage = isLeaderOfPendingTeam
        ? 'Are you sure you want to cancel your team application? This will permanently delete the team.'
        : 'Are you sure you want to leave this team?';

    const handleClick = () => {
        if (window.confirm(confirmMessage)) {
            mutation.mutate();
        }
    };

    return (
        <div>
            <button
                onClick={handleClick}
                disabled={mutation.isPending}
                className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 disabled:bg-gray-400 text-sm font-semibold"
            >
                {buttonText}
            </button>
            {mutation.isError && <p className="text-red-500 text-xs mt-1">Failed to leave team. The last leader cannot leave an approved team.</p>}
        </div>
    );
}