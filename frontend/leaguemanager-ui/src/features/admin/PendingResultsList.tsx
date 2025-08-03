import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../api/apiClient';
import type { Result } from '../../types';
import { Link } from 'react-router-dom';
import { useState } from 'react';

// Function to fetch pending results (unchanged)
const fetchPendingResults = async (): Promise<Result[]> => {
  const { data } = await apiClient.get('/results/pending');
  return data;
};

// Function to update a result's status (unchanged)
const updateResultStatus = async ({ resultId, status }: { resultId: number; status: number }) => {
  await apiClient.patch(`/results/${resultId}/status`, { status });
};

export default function PendingResultsList() {
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null); // Add state for error messages

  const { data: pendingResults, isLoading, isError } = useQuery({
    queryKey: ['pendingResults'],
    queryFn: fetchPendingResults,
  });

  // Set up the mutation for updating a result's status
  const updateStatusMutation = useMutation({
    mutationFn: updateResultStatus,
    onSuccess: () => {
      setError(null); // Clear any previous errors
      // When a result is updated, refetch the list to keep it up to date
      queryClient.invalidateQueries({ queryKey: ['pendingResults'] });
    },
    onError: (err: any) => {
      // Display any errors from the backend
      setError(err.response?.data?.message || 'An unexpected error occurred.');
    },
  });

  if (isLoading) return <div>Loading pending results...</div>;
  if (isError) return <div className="text-red-500">Failed to load pending results.</div>;

  return (
    <div>
      <h2 className="text-xl font-semibold mb-2">Pending Result Approvals</h2>
      
      {/* Display the error message if it exists */}
      {error && <div className="p-2 mb-4 text-sm text-red-700 bg-red-100 rounded-md">{error}</div>}

      {pendingResults && pendingResults.length > 0 ? (
        <ul className="space-y-2">
          {pendingResults.map((result) => (
            <li key={result.id} className="flex justify-between items-center p-2 border rounded">
              <Link to={`/fixtures/${result.fixtureId}`} className="hover:underline">
                Fixture #{result.fixtureId}: {result.homeScore} - {result.awayScore}
              </Link>
              <div className="space-x-2">
                <button
                  // Call the mutation with the correct status for "Approve"
                  onClick={() => {
                    setError(null);
                    updateStatusMutation.mutate({ resultId: result.id, status: 1 }); // 1 = Approved
                  }}
                  disabled={updateStatusMutation.isPending}
                  className="px-3 py-1 bg-green-500 text-white rounded hover:bg-green-600 disabled:bg-gray-400"
                >
                  Approve
                </button>
                <button
                  // Call the mutation with the correct status for "Dispute"
                  onClick={() => {
                    setError(null);
                    updateStatusMutation.mutate({ resultId: result.id, status: 2 }); // 2 = Disputed
                  }}
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