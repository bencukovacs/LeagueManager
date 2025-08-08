import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { RosterRequestResponseDto } from '../../types';

// Function to fetch pending join requests for the leader's team
const fetchPendingJoinRequests = async (): Promise<RosterRequestResponseDto[]> => {
  const { data } = await apiClient.get('/roster-requests/my-team/pending');
  return data;
};

// Function to approve a request
const approveRequest = async (requestId: number) => {
  await apiClient.patch(`/roster-requests/${requestId}/approve`);
};

// Function to reject a request
const rejectRequest = async (requestId: number) => {
  await apiClient.patch(`/roster-requests/${requestId}/reject`);
};

export default function JoinRequestsList() {
  const queryClient = useQueryClient();

  const { data: requests, isLoading } = useQuery({
    queryKey: ['pendingJoinRequests'],
    queryFn: fetchPendingJoinRequests,
  });

  const approveMutation = useMutation({
    mutationFn: approveRequest,
    onSuccess: () => {
      // On success, refetch both the requests list and the team's roster
      queryClient.invalidateQueries({ queryKey: ['pendingJoinRequests'] });
      queryClient.invalidateQueries({ queryKey: ['roster'] });
    },
  });

  const rejectMutation = useMutation({
    mutationFn: rejectRequest,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['pendingJoinRequests'] });
    },
  });

  if (isLoading) return <p>Loading join requests...</p>;

  return (
    <div className="mt-6 p-4 border rounded-lg">
      <h3 className="text-xl font-semibold mb-4">Pending Join Requests</h3>
      {requests && requests.length > 0 ? (
        <ul className="space-y-2">
          {requests.map((req) => (
            <li key={req.id} className="flex justify-between items-center p-2 bg-gray-100 rounded">
              <span>{req.userName} has requested to join your team.</span>
              <div className="space-x-2">
                <button
                  onClick={() => approveMutation.mutate(req.id)}
                  disabled={approveMutation.isPending || rejectMutation.isPending}
                  className="px-3 py-1 bg-green-500 text-white rounded hover:bg-green-600 disabled:bg-gray-400"
                >
                  Approve
                </button>
                <button
                  onClick={() => rejectMutation.mutate(req.id)}
                  disabled={approveMutation.isPending || rejectMutation.isPending}
                  className="px-3 py-1 bg-red-500 text-white rounded hover:bg-red-600 disabled:bg-gray-400"
                >
                  Reject
                </button>
              </div>
            </li>
          ))}
        </ul>
      ) : (
        <p className="text-gray-500">No pending join requests.</p>
      )}
    </div>
  );
}