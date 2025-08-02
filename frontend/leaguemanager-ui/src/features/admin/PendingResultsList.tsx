import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { Result } from '../../types';
import { Link } from 'react-router-dom';

// Function to fetch pending results
const fetchPendingResults = async (): Promise<Result[]> => {
  const { data } = await apiClient.get('/results/pending');
  return data;
};

// Function to update a result's status (approve or dispute)
const updateResultStatus = async ({ resultId, status }: { resultId: number; status: number }) => {
  await apiClient.patch(`/results/${resultId}/status`, { status });
};

export default function PendingResultsList() {
  const queryClient = useQueryClient();

  const { data: pendingResults, isLoading, isError } = useQuery({
    queryKey: ['pendingResults'],
    queryFn: fetchPendingResults,
  });

  const updateStatusMutation = useMutation({
    mutationFn: updateResultStatus,
    onSuccess: () => {
      // When a result is updated, refetch the list
      queryClient.invalidateQueries({ queryKey: ['pendingResults'] });
    },
  });

  if (isLoading) return <div>Loading pending results...</div>;
  if (isError) return <div className="text-red-500">Failed to load pending results.</div>;

  return (
    <div>
      <h2 className="text-xl font-semibold mb-2">Pending Result Approvals</h2>
      {pendingResults && pendingResults.length > 0 ? (
        <ul className="space-y-2">
          {pendingResults.map((result) => (
            <li key={result.id} className="flex justify-between items-center p-2 border rounded">
              <Link to={`/fixtures/${result.fixtureId}`} className="hover:underline">
                Fixture #{result.fixtureId}: {result.homeScore} - {result.awayScore}
              </Link>
              <div className="space-x-2">
                <button
                  onClick={() => updateStatusMutation.mutate({ resultId: result.id, status: 1 })} // 1 = Approved
                  disabled={updateStatusMutation.isPending}
                  className="px-3 py-1 bg-green-500 text-white rounded hover:bg-green-600 disabled:bg-gray-400"
                >
                  Approve
                </button>
                <button
                  onClick={() => updateStatusMutation.mutate({ resultId: result.id, status: 2 })} // 2 = Disputed
                  disabled={updateStatusMutation.isPending}
                  className="px-3 py-1 bg-red-500 text-white rounded hover:bg-red-600 disabled:bg-gray-400"
                >
                  Dispute
                </button>
              </div>
            </li>
          ))}
        </ul>
      ) : (
        <p>No results are currently waiting for approval.</p>
      )}
    </div>
  );
}