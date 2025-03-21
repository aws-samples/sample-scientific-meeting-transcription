/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

import { createRouter, createWebHistory } from 'vue-router'
import MeetingAssistant from '@/src/views/MeetingAssistant.vue'
import Login from '@views/Login.vue'
import store from '../store'
import { fetchAuthSession } from 'aws-amplify/auth';

const routes = [

  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: { requiresAuth: false }
  },
  {
    path: '/forgot-password',
    name: 'ForgotPassword',
    component: () => import('@views/ForgotPassword.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    component: () => import('@views/Dashboard.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: 'teams',
        name: 'Teams',
        component: () => import('@views/Teams.vue'),
      },
      {
        path: '/meeting-assistant',
        name: 'MeetingAssistant',
        component: MeetingAssistant,
      },
      {
        path: 'custom-models',
        name: 'CustomModels',
        component: () => import('@views/CustomModels.vue'),
      },
      {
        path: 'custom-vocabularies',
        name: 'CustomVocabularies',
        component: () => import('@views/CustomVocabularies.vue'),
      },
      {
        path: 'meetings',
        name: 'Meetings',
        component: () => import('@/src/views/Meetings/Meetings.vue'),
      },
      {
        path: 'prompt-sets',
        name: 'PromptSets',
        component: () => import('@views/PromptSets.vue'),
      },
      {
        path: '',
        redirect: '/teams'
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const savedTeamId = localStorage.getItem('selectedTeamId')
  if (savedTeamId && !store.getters.selectedTeamId) {
    store.dispatch('setSelectedTeam', savedTeamId)
  }


  if (to.matched.some(record => record.meta.requiresAuth)) {
    if (!fetchAuthSession()) {
      next('/login')
    } else {
      next()
    }
  } else {
    next()
  }
})

export default router