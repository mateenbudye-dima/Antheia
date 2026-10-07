import { useMutation, useQueryClient } from '@tanstack/react-query';
import { blendsApi } from '../api/blendsApi';
import { blendKeys } from '../api/blendKeys';

export const useBlendWorkflowMutations = (blendId: number) => {
  const queryClient = useQueryClient();

  const invalidateBlend = () => {
    queryClient.invalidateQueries({ queryKey: blendKeys.detail(blendId) });
  };

  // 1. Submit for Review
  const submitForReviewMutation = useMutation({
    mutationKey: ['blend', blendId, 'submitForReview'],
    mutationFn: () => blendsApi.submitBlendForReview(blendId),
    onSuccess: invalidateBlend,
  });

  // 2. Submit for Approval
  const submitForApprovalMutation = useMutation({
    mutationKey: ['blend', blendId, 'submitForApproval'],
    mutationFn: () => blendsApi.submitBlendForApproval(blendId),
    onSuccess: invalidateBlend,
  });

  // 3. Review (Self Review / Reviewer Action)
  const reviewBlendMutation = useMutation({
    mutationKey: ['blend', blendId, 'review'],
    mutationFn: (comments: string) => blendsApi.reviewBlend(blendId, comments),
    onSuccess: invalidateBlend,
  });

  // 4. Approve (Self Approve / Approver Action)
  const approveBlendMutation = useMutation({
    mutationKey: ['blend', blendId, 'approve'],
    mutationFn: (comments: string) => blendsApi.approveBlend(blendId, comments),
    onSuccess: invalidateBlend,
  });

  return {
    submitForReview: submitForReviewMutation.mutateAsync,
    submitForApproval: submitForApprovalMutation.mutateAsync,
    reviewBlend: reviewBlendMutation.mutateAsync,
    approveBlend: approveBlendMutation.mutateAsync,
    isSubmitting:
      submitForReviewMutation.isPending ||
      submitForApprovalMutation.isPending ||
      reviewBlendMutation.isPending ||
      approveBlendMutation.isPending,
  };
};