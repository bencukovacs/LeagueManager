import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { RosterRequestResponseDto } from '../../types';

// API Functions
const fetchMyPendingRequests = async (): Promise<RosterRequestResponseDto[]> => {
  const { data } = await apiClient.get('/me/roster-requests');
  return data;
};

const cancelRequest = async (requestId: number) => {
  await apiClient.delete(`/roster-requests/${requestId}`);
};

export default function MyPendingRequests() {
  const queryClient = useQueryClient();

  const { data: requests, isLoading } = useQuery({
    queryKey: ['myPendingRequests'],
    queryFn: fetchMyPendingRequests,
  });

  const mutation = useMutation({
    mutationFn: cancelRequest,
    onSuccess: () => {
      // Refetch the requests list after a cancellation
      queryClient.invalidateQueries({ queryKey: ['myPendingRequests'] });
    },
  });

  if (isLoading) return <p>Loading your pending requests...</p>;
  if (!requests || requests.length === 0) return null;

  return (
    <div className="mt-8 p-4 border rounded-lg bg-gray-50">
      <h3 className="text-xl font-semibold mb-4">Your Pending Requests</h3>
      <ul className="space-y-2">
        {requests.map((req) => (
          <li key={req.id} className="flex justify-between items-center p-2 border rounded bg-white">
            <span>Request to join <span className="font-semibold">{req.teamName}</span> is {req.status}.</span>
            <button
              onClick={() => mutation.mutate(req.id)}
              disabled={mutation.isPending}
              className="px-3 py-1 text-sm bg-red-500 text-white rounded hover:bg-red-600 disabled:bg-gray-400"
            >
              Cancel
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
}