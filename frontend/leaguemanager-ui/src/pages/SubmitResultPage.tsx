import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import apiClient from '../api/apiClient';
import type { FixtureResponseDto, MyTeamAndConfigResponse } from '../types';
import { useState, useMemo } from 'react';
import { useAuth } from '../contexts/AuthContext';

const fetchFixture = async (fixtureId: string): Promise<FixtureResponseDto> => {
  const { data } = await apiClient.get(`/fixtures/${fixtureId}`);
  return data;
};

const submitResult = async (payload: { fixtureId: string; data: any }) => {
  await apiClient.post(`/fixtures/${payload.fixtureId}/results`, payload.data);
};

const fetchMyTeamAndConfig = async (): Promise<MyTeamAndConfigResponse | null> => {
    try {
        const { data } = await apiClient.get('/my-team');
        return data;
    } catch (error) {
        return null;
    }
};

export default function SubmitResultPage() {
  const { fixtureId } = useParams<{ fixtureId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();

  const [goalscorers, setGoalscorers] = useState<number[]>([]);
  const [ownMom, setOwnMom] = useState<number | undefined>();
  const [oppMom, setOppMom] = useState<number | undefined>();

  const { data: fixture, isLoading, isError } = useQuery({
    queryKey: ['fixture', fixtureId],
    queryFn: () => fetchFixture(fixtureId!),
    enabled: !!fixtureId,
  });

  const { data: response } = useQuery({
    queryKey: ['myTeamAndConfig'],
    queryFn: fetchMyTeamAndConfig,
    enabled: !!user,
  });
  const myTeamData = response?.myTeam;

  const homeScore = useMemo(() => {
    if (!fixture) return 0;
    return goalscorers.filter(id => fixture.homeTeamRoster.some(p => p.id === id)).length;
  }, [goalscorers, fixture]);

  const awayScore = useMemo(() => {
    if (!fixture) return 0;
    return goalscorers.filter(id => fixture.awayTeamRoster.some(p => p.id === id)).length;
  }, [goalscorers, fixture]);

  const { yourTeamRoster, opponentRoster } = useMemo(() => {
    if (!myTeamData?.team || !fixture) {
      return { yourTeamRoster: [], opponentRoster: [] };
    }
    if (myTeamData.team.id === fixture.homeTeam.id) {
      return { yourTeamRoster: fixture.homeTeamRoster, opponentRoster: fixture.awayTeamRoster };
    } else {
      return { yourTeamRoster: fixture.awayTeamRoster, opponentRoster: fixture.homeTeamRoster };
    }
  }, [myTeamData, fixture]);

  const mutation = useMutation({
    mutationFn: submitResult,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['myTeamFixtures'] });
      navigate('/my-team/fixtures');
    },
  });

  const addGoal = (playerId: number) => {
    setGoalscorers(prev => [...prev, playerId]);
  };

  const removeGoal = (playerId: number) => {
    setGoalscorers(prev => {
      const index = prev.lastIndexOf(playerId);
      if (index > -1) {
        const newArr = [...prev];
        newArr.splice(index, 1);
        return newArr;
      }
      return prev;
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!ownMom || !oppMom) {
      alert("Please select a Man of the Match for both teams.");
      return;
    }
    const payload = {
      homeScore,
      awayScore,
      goalscorers: goalscorers.map(id => ({ playerId: id })),
      momVote: {
        votedForOwnPlayerId: ownMom,
        votedForOpponentPlayerId: oppMom,
      },
    };
    mutation.mutate({ fixtureId: fixtureId!, data: payload });
  };

  if (isLoading) return <div className="p-4">Loading fixture...</div>;
  if (isError || !fixture) return <div className="p-4 text-red-500">Error loading fixture.</div>;

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-3xl font-bold mb-2">Submit Result</h1>
      <p className="text-xl mb-6">{fixture.homeTeam.name} vs {fixture.awayTeam.name}</p>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="flex items-center justify-center space-x-4 p-4 bg-sky-400 text-white rounded-lg">
          <span className="text-5xl font-bold">{homeScore}</span>
          <span className="text-3xl">-</span>
          <span className="text-5xl font-bold">{awayScore}</span>
        </div>

        <div>
          <h2 className="text-xl font-semibold">Goalscorers</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2">
            <div>
              <h3 className="font-bold">{fixture.homeTeam.name}</h3>
              {fixture.homeTeamRoster.map(p => (
                <div key={p.id} className="flex items-center justify-between p-1">
                  <span>{p.name} ({goalscorers.filter(id => id === p.id).length})</span>
                  <div className="flex items-center gap-2">
                    <button type="button" onClick={() => removeGoal(p.id)} className="w-6 h-6 bg-red-500 text-white rounded-full">-</button>
                    <button type="button" onClick={() => addGoal(p.id)} className="w-6 h-6 bg-green-500 text-white rounded-full">+</button>
                  </div>
                </div>
              ))}
            </div>
            <div>
              <h3 className="font-bold">{fixture.awayTeam.name}</h3>
              {fixture.awayTeamRoster.map(p => (
                <div key={p.id} className="flex items-center justify-between p-1">
                  <span>{p.name} ({goalscorers.filter(id => id === p.id).length})</span>
                  <div className="flex items-center gap-2">
                    <button type="button" onClick={() => removeGoal(p.id)} className="w-6 h-6 bg-red-500 text-white rounded-full">-</button>
                    <button type="button" onClick={() => addGoal(p.id)} className="w-6 h-6 bg-green-500 text-white rounded-full">+</button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div>
          <h2 className="text-xl font-semibold">Man of the Match</h2>
          <div className="grid grid-cols-2 gap-4 mt-2">
            <div>
              <label className="font-bold">Your Team's MOM</label>
              <select onChange={e => setOwnMom(parseInt(e.target.value))} className="w-full p-2 border rounded mt-1" required>
                <option value="">Select a player</option>
                {yourTeamRoster.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
              </select>
            </div>
            <div>
              <label className="font-bold">Opponent's MOM</label>
              <select onChange={e => setOppMom(parseInt(e.target.value))} className="w-full p-2 border rounded mt-1" required>
                <option value="">Select a player</option>
                {opponentRoster.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
              </select>
            </div>
          </div>
        </div>
        
        <button type="submit" disabled={mutation.isPending} className="px-6 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:bg-gray-400">
          Submit Final Result
        </button>
        {mutation.isError && <p className="text-red-500 mt-2">Error: An error occurred while submitting the result.</p>}
      </form>
    </div>
  );
}