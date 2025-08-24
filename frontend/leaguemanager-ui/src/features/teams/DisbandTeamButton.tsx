import { useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';

// API function to disband the team
const disbandMyTeam = async () => {
    await apiClient.delete('/my-team/disband');
};

export default function DisbandTeamButton() {
    const queryClient = useQueryClient();

    const mutation = useMutation({
        mutationFn: disbandMyTeam,
        onSuccess: () => {
            // On success, refetch the user's team data, which will redirect them to the lobby
            queryClient.invalidateQueries({ queryKey: ['myTeamAndConfig'] });
            queryClient.invalidateQueries({ queryKey: ['myTeamStatus'] });
        },
    });

    const handleClick = () => {
        if (window.confirm('Are you sure you want to disband this team? This will remove all members and permanently delete the team. This action cannot be undone.')) {
            mutation.mutate();
        }
    };

    return (
        <div>
            <button
                onClick={handleClick}
                disabled={mutation.isPending}
                className="px-4 py-2 bg-gray-700 text-white rounded-md hover:bg-gray-800 disabled:bg-gray-400 text-sm font-semibold"
            >
                Disband Team
            </button>
            {mutation.isError && <p className="text-red-500 text-xs mt-1">Failed to disband team.</p>}
        </div>
    );
}